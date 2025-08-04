using ChatsService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatsService.Application.Features.Users.RollBackAvatar;

public class RollBackUserAvatarCommandHandler(
    ChatsDbContext dbContext,
    ILogger<RollBackUserAvatarCommandHandler> logger) : IRequestHandler<RollBackUserAvatarCommand>
{
    public async Task Handle(RollBackUserAvatarCommand request, CancellationToken cancellationToken)
    {
        var user = await dbContext.UserSnapshots
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
        {
            logger.LogInformation($"User with id: {request.UserId} not found");
            return;
        }
        
        user.AvatarImageName = request.PreviousAvatarName;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}