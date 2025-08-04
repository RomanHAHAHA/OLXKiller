using Common.Application.Options;
using Common.Domain.Enums;
using Common.Domain.Interfaces;
using Common.Domain.Models.Results;
using Common.Infrastructure.Messaging.Events.SystemAction;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Options;
using UsersService.Domain.Interfaces;

namespace UsersService.Application.Features.Users.UpdatePassword;

public class UpdatePasswordCommandHandler(
    IUsersRepository usersRepository,
    IPublishEndpoint publishEndpoint, 
    IPasswordHasher passwordHasher,
    IOptions<ServiceOptions> serviceOptions) : IRequestHandler<UpdatePasswordCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(UpdatePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await usersRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return ApiResponse.NotFound("Unable to complete the request");
        }

        if (!passwordHasher.Verify(request.UpdatePasswordDto.OldPassword, user.PasswordHash))
        {
            await OnIncorrectPasswordEntered(user.Id, cancellationToken);
            await usersRepository.SaveChangesAsync(cancellationToken);
            
            return ApiResponse.BadRequest("Incorrect old password");
        }
        
        user.PasswordHash = passwordHasher.HashPassword(request.UpdatePasswordDto.NewPassword);
        await OnPasswordUpdated(user.Id, cancellationToken);

        var updated = await usersRepository.SaveChangesAsync(cancellationToken);

        return updated ? ApiResponse.Ok() : ApiResponse.InternalServerError();
    }
    
    private async Task OnIncorrectPasswordEntered(Guid userId, CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(
            new SystemActionEvent
            {
                CorrelationId = Guid.NewGuid(),
                SenderServiceName = serviceOptions.Value.Name,
                UserId = userId,
                ActionType = ActionType.IncorrectPasswordAttempt,
                Message = "Incorrect password entered"
            }, 
            cancellationToken);
    }
    
    private async Task OnPasswordUpdated(Guid userId, CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(
            new SystemActionEvent
            {
                CorrelationId = Guid.NewGuid(),
                SenderServiceName = serviceOptions.Value.Name,
                UserId = userId,
                ActionType = ActionType.Update,
                Message = "Password reset"
            },
            cancellationToken);
    }
}