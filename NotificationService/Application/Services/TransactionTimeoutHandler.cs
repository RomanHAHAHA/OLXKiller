using NotificationService.Domain.Interfaces;

namespace NotificationService.Application.Services;

public class TransactionTimeoutHandler(
    IRedisService redisService,
    ILogger<TransactionTimeoutHandler> logger) : ITransactionTimeoutHandler
{
    public async Task<bool> TryHandleTimeoutAsync(
        ITransactionCoordinator coordinator,
        Guid correlationId,
        Func<Task> onTimeout,
        CancellationToken cancellationToken)
    {
        var lockKey = coordinator.GetLockKey(correlationId);
        var completedKey = coordinator.GetCompletedKey(correlationId);
        
        var gotLock = await redisService.AcquireLockAsync(
            lockKey, 
            TimeSpan.FromSeconds(3),
            maxRetries: 30,
            delayBetweenRetries: TimeSpan.FromMilliseconds(100),
            cancellationToken);

        if (!gotLock)
        {
            return false;
        }
        
        try
        {
            var isCompleted = await redisService.GetFieldValueAsync<bool>(completedKey, "isCompleted");
            
            if (isCompleted)
            {
                logger.LogInformation($"Transaction already completed, ignoring timeout for {correlationId}");
                return false;
            }
            
            var completedServices = await redisService.GetAllFieldsAsync(correlationId.ToString());

            if (completedServices.SetEquals(coordinator.RequiredServices))
            {
                logger.LogInformation("Not all services completed their actions");
                return false;
            }
            
            await onTimeout();
            await redisService.RemoveAsync(correlationId.ToString());
            
            return true;
        }
        finally
        {
            await redisService.ReleaseLockAsync(lockKey);
        }
    }
}