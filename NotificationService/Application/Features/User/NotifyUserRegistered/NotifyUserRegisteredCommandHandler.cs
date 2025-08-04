using Common.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using NotificationService.API.Hubs;
using NotificationService.Domain.Interfaces;

namespace NotificationService.Application.Features.User.NotifyUserRegistered;

public class NotifyUserRegisteredCommandHandler(
    IRedisLockService redisLockService,
    IHashCacheService hashCacheService,
    IHubContext<NotificationHub, INotificationClient> hubContext) : IRequestHandler<NotifyUserRegisteredCommand>
{
    private static readonly string[] RequiredServices =
    [
        UserRegistrationRequiredServices.ReviewsService,
        UserRegistrationRequiredServices.OrdersService,
        UserRegistrationRequiredServices.ChatsService,
    ];
    
    public async Task Handle(NotifyUserRegisteredCommand request, CancellationToken cancellationToken)
    {
        var lockKey = $"lock:user-registered:{request.CorrelationId}";
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
                    .Clients(request.ConnectionId)
                    .NotifyUserRegistered();

                await hashCacheService.RemoveAsync(request.CorrelationId.ToString());
            }
        }
        finally
        {
            await redisLockService.ReleaseLockAsync(lockKey);
        }
    }
}