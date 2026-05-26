using NotificationService.Domain.Interfaces;
using NotificationService.Domain.Models;

namespace NotificationService.Application.Services;

public class TransactionCompleter(
    IRedisService redisService,
    ILogger<TransactionCompleter> logger) : ITransactionCompleter
{
    public async Task<bool> TryCompleteAsync(
        ITransactionCoordinator transactionCoordinator,
        TransactionDetails transactionDetails,
        Func<Task> onComplete,
        CancellationToken cancellationToken)
    {
        var correlationId = transactionDetails.CorrelationId;
        var lockKey = transactionCoordinator.GetLockKey(correlationId);
        var completedKey = transactionCoordinator.GetCompletedKey(correlationId);
        
        await redisService.SetFieldAsync(
            correlationId.ToString(),
            transactionDetails.SenderServiceName,
            TimeSpan.FromHours(1));
        
        var gotLock = await redisService.AcquireLockAsync(
            lockKey, 
            TimeSpan.FromSeconds(1),
            maxRetries: 5, 
            delayBetweenRetries: TimeSpan.FromMilliseconds(50),
            cancellationToken);

        if (!gotLock)
        {
            return false;
        }

        try
        {
            var succeededServices = await redisService.GetAllFieldsAsync(correlationId.ToString());
            var allSucceeded = succeededServices.SetEquals(transactionCoordinator.RequiredServices);

            if (!allSucceeded)
            {
                return false;
            }
            
            await redisService.SetFieldAsync(
                completedKey,
                "isCompleted",
                true,
                TimeSpan.FromMinutes(5));
            
            await onComplete();
            await redisService.RemoveAsync(correlationId.ToString());
            
            logger.LogInformation($"Transaction completed: {correlationId}");
            
            return true;
        }
        finally
        {
            await redisService.ReleaseLockAsync(lockKey);
        }
    }
}