using MediatR;
using OrdersService.Domain.Interfaces;

namespace OrdersService.Application.Features.Users.Delete;

public class DeleteUserCommandHandler(
    IUsersRepository usersRepository,
    ILogger<DeleteUserCommandHandler> logger) : IRequestHandler<DeleteUserCommand>
{
    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await usersRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            logger.LogInformation($"User with id: {request.UserId} was not found");
            return;
        }
        
        usersRepository.Delete(user);
        var deleted = await usersRepository.SaveChangesAsync(cancellationToken);
        
        var message = deleted ? 
            "User deleted" : "Failed to delete user";
        
        logger.LogInformation(message);
    }
}