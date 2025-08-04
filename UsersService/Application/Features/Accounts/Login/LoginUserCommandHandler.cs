using Common.Application.Options;
using Common.Domain.Enums;
using Common.Domain.Interfaces;
using Common.Domain.Models.Results;
using Common.Infrastructure.Messaging.Events.SystemAction;
using Common.Infrastructure.Messaging.Events.User;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Options;
using UsersService.Domain.Interfaces;

namespace UsersService.Application.Features.Accounts.Login;

public class LoginUserCommandHandler(
    IUsersRepository usersRepository,
    IJwtProvider jwtProvider,
    IPasswordHasher passwordHasher,
    IPublishEndpoint publisher,
    IOptions<ServiceOptions> serviceOptions) : IRequestHandler<LoginUserCommand, ApiResponse<string>>
{
    public async Task<ApiResponse<string>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await usersRepository.GetByEmailAsync(request.UserLoginDto.Email, cancellationToken);

        if (user is null)
        {
            return ApiResponse<string>.NotFound("User with such email");
        }

        if (!user.EmailConfirmed)
        {
            return ApiResponse<string>.Conflict("You have to confirm your email");
        }

        if (!passwordHasher.Verify(request.UserLoginDto.Password, user.PasswordHash))
        {
            await OnIncorrectPasswordEntered(user.Id, cancellationToken);
            return ApiResponse<string>.BadRequest("Incorrect password");
        }

        await OnUserLoggedId(user.Id, cancellationToken);
        var token = await jwtProvider.GenerateTokenAsync(user, cancellationToken);

        return token;
    }

    private async Task OnUserLoggedId(Guid userId, CancellationToken cancellationToken)
    {
        await publisher.Publish(
            new UserLoggedInEvent
            {
                CorrelationId = Guid.NewGuid(),
                SenderServiceName = serviceOptions.Value.Name,
                UserId = userId,
                LogInTime = DateTime.UtcNow,
            }, 
            cancellationToken);
        
        await usersRepository.SaveChangesAsync(cancellationToken);
    }    
    
    private async Task OnIncorrectPasswordEntered(Guid userId, CancellationToken cancellationToken)
    {
        await publisher.Publish(
            new SystemActionEvent
            {
                CorrelationId = Guid.NewGuid(),
                SenderServiceName = serviceOptions.Value.Name,
                UserId = userId,
                ActionType = ActionType.IncorrectPasswordAttempt,
                Message = "Incorrect password entered",
            }, 
            cancellationToken);
        
        await usersRepository.SaveChangesAsync(cancellationToken);
    }
}