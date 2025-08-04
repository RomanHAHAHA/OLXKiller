using MediatR;
using Microsoft.AspNetCore.SignalR;
using NotificationService.API.Hubs;
using NotificationService.Domain.Interfaces;

namespace NotificationService.Application.Features.Product.NotifyProductCreationFailed;

public class NotifyProductSnapshotCreationFailedCommandHandler(
    IHubContext<NotificationHub, INotificationClient> hubContext,
    IRedisLockService redisLockService) : IRequestHandler<NotifyProductSnapshotCreationFailedCommand>
{
    public async Task Handle(NotifyProductSnapshotCreationFailedCommand request, CancellationToken cancellationToken)
    {
        await hubContext.Clients
            .User(request.UserId.ToString())
            .NotifyProductCreationFailed("Unexpected server error occured during the request. Please try again later.");
        
        var lockKey = $"lock:product-created:{request.CorrelationId}";
        await redisLockService.ReleaseLockAsync(lockKey);
    }
}