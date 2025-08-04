using Common.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using NotificationService.API.Hubs;
using NotificationService.Domain.Interfaces;

namespace NotificationService.Application.Features.Product.NotifyProductUpdated;

public class NotifyProductSnapshotUpdatedCommandHandler(
    IHubContext<NotificationHub, INotificationClient> hubContext,
    IHashCacheService hashCacheService,
    IRedisLockService redisLockService) : IRequestHandler<NotifyProductSnapshotUpdatedCommand>
{
    private static readonly string[] RequiredServices =
    [
        ProductCreationRequiredServices.CartsService,
        ProductCreationRequiredServices.OrdersService,
        ProductCreationRequiredServices.ReviewsService
    ];
    
    public async Task Handle(NotifyProductSnapshotUpdatedCommand request, CancellationToken cancellationToken)
    {
        await hashCacheService.SetFieldAsync(
            request.CorrelationId.ToString(),
            request.SenderServiceName,
            TimeSpan.FromHours(1),
            cancellationToken);
        
        var lockKey = $"lock:product-updated:{request.CorrelationId}";
        var gotLock = await redisLockService.AcquireLockWithRetryAsync(
            lockKey, 
            TimeSpan.FromSeconds(1), 
            maxRetries: 5, 
            delayBetweenRetries: TimeSpan.FromMilliseconds(50),
            cancellationToken);

        if (!gotLock)
        {
            return;
        }

        try
        {
            var succeededServices = await hashCacheService
                .GetAllFieldsAsync(request.CorrelationId.ToString(), cancellationToken);

            var allSucceeded = succeededServices.SetEquals(RequiredServices);
            
            if (allSucceeded)
            {
                await redisLockService.ReleaseLockAsync(lockKey);

                await hubContext.Clients
                    .User(request.UserId.ToString())
                    .NotifyProductUpdated(request.ProductId, "Product successfully updated");

                await hashCacheService.RemoveAsync(request.CorrelationId.ToString());
            }
        }
        finally
        {
            if (gotLock)
            {
                await redisLockService.ReleaseLockAsync(lockKey);
            }
        }
    }
}