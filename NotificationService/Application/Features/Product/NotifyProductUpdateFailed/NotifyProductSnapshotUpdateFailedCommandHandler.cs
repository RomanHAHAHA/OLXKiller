using MediatR;
using Microsoft.AspNetCore.SignalR;
using NotificationService.API.Hubs;
using NotificationService.Domain.Interfaces;

namespace NotificationService.Application.Features.Product.NotifyProductUpdateFailed;

public class NotifyProductSnapshotUpdateFailedCommandHandler(
    IHubContext<NotificationHub, INotificationClient> hubContext,
    IRedisLockService redisLockService) : IRequestHandler<NotifyProductSnapshotUpdateFailedCommand>
{
    public async Task Handle(NotifyProductSnapshotUpdateFailedCommand request, CancellationToken cancellationToken)
    {
        await hubContext.Clients
            .User(request.UserId.ToString())
            .NotifyProductUpdateFailed("Unexpected server error occured during the request. Please try again later.");
        
        var lockKey = $"lock:product-updated:{request.CorrelationId}";
        await redisLockService.ReleaseLockAsync(lockKey);
    }
}