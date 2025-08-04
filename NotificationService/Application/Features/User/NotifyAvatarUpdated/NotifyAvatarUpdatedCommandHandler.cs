using Common.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using NotificationService.API.Hubs;
using NotificationService.Domain.Interfaces;

namespace NotificationService.Application.Features.User.NotifyAvatarUpdated;

public class NotifyAvatarUpdatedCommandHandler(
    IHashCacheService hashCacheService,
    IRedisLockService redisLockService,
    IHubContext<NotificationHub, INotificationClient> hubContext) : IRequestHandler<NotifyAvatarUpdatedCommand>
{
    private static readonly string[] RequiredServices =
    [
        AvatarUpdateRequiredServices.OrdersService,
        AvatarUpdateRequiredServices.ReviewsService,
        AvatarUpdateRequiredServices.ChatsService,
    ];
    
    public async Task Handle(NotifyAvatarUpdatedCommand request, CancellationToken cancellationToken)
    {
        var lockKey = $"lock:avatar-updated:{request.CorrelationId}";
        var gotLock = await redisLockService.AcquireLockWithRetryAsync(
            lockKey, 
            TimeSpan.FromSeconds(2), 
            maxRetries: 10, 
            delayBetweenRetries: TimeSpan.FromMilliseconds(50),
            cancellationToken);

        if (!gotLock)
        {
            return;
        }

        try
        {
            await hashCacheService.SetFieldAsync(
                request.CorrelationId.ToString(),
                request.SenderServiceName,
                TimeSpan.FromHours(1),
                cancellationToken);

            var successServices = await hashCacheService.GetAllFieldsAsync(
                request.CorrelationId.ToString(),
                cancellationToken);

            var allSucceeded = successServices.SetEquals(RequiredServices);
            
            if (allSucceeded)
            {
                await hubContext.Clients
                    .User(request.UserId.ToString())
                    .NotifyUserAvatarUpdated("Avatar successfully updated!");
                
                await hashCacheService.RemoveAsync(request.CorrelationId.ToString());
            }
        }
        finally
        {
            await redisLockService.ReleaseLockAsync(lockKey);
        }
    }
}