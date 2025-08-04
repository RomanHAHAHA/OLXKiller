using MediatR;
using ProductsService.Domain.Entities;
using ProductsService.Domain.Interfaces;

namespace ProductsService.Application.Features.Users.Create;

public class CreateUserCommandHandler(
    IUsersRepository usersRepository,
    ILogger<CreateUserCommandHandler> logger) : IRequestHandler<CreateUserCommand>
{
    public async Task Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = new UserSnapshot
        {
            Id = request.UserId,
            NickName = request.NickName,
            CreatedAt = request.RegisterDate
        };

        await usersRepository.CreateAsync(user, cancellationToken);
        var created = await usersRepository.SaveChangesAsync(cancellationToken);
        
        var message = created ? 
            "User created successfully." :
            "Failed to create user.";
        
        logger.LogInformation(message);
    }
}