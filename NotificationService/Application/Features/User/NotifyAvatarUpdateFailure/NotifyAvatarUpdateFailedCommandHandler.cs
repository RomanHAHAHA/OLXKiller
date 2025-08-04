using MediatR;
using Microsoft.AspNetCore.SignalR;
using NotificationService.API.Hubs;
using NotificationService.Domain.Interfaces;

namespace NotificationService.Application.Features.User.NotifyAvatarUpdateFailure;

public class NotifyAvatarUpdateFailedCommandHandler(
    IHubContext<NotificationHub, INotificationClient> hubContext,
    IRedisLockService redisLockService) : IRequestHandler<NotifyAvatarUpdateFailedCommand>
{
    public async Task Handle(NotifyAvatarUpdateFailedCommand request, CancellationToken cancellationToken)
    {
        await hubContext.Clients
            .User(request.UserId.ToString())
            .NotifyUserAvatarUpdateFailed("Unexpected server error occured during the request. Please try again later.");
        
        var lockKey = $"lock:avatar-updated:{request.CorrelationId}";
        await redisLockService.ReleaseLockAsync(lockKey);
    }
}