using ChatsService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatsService.Application.Features.Users.Delete;

public class DeleteUserCommandHandler(
    ChatsDbContext dbContext,
    ILogger<DeleteUserCommandHandler> logger) : IRequestHandler<DeleteUserCommand>
{
    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await dbContext.UserSnapshots
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
        {
            logger.LogInformation($"User with id: {request.UserId} was not found");
            return;
        }
        
        dbContext.UserSnapshots.Remove(user);
        var deleted = await dbContext.SaveChangesAsync(cancellationToken) > 0;
        
        logger.LogInformation(deleted ? "User deleted" : "Failed to delete user");
    }
}