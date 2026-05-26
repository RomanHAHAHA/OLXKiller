using NotificationService.Domain.Interfaces;

namespace NotificationService.Application.Services;

public class TransactionFailureHandler(IRedisService redisService) : ITransactionFailureHandler
{
    public async Task<bool> TryHandleFailureAsync(
        ITransactionCoordinator coordinator,
        Guid correlationId,
        Func<Task> onFailure)
    {
        var lockKey = coordinator.GetLockKey(correlationId);
        var completedKey = coordinator.GetCompletedKey(correlationId);
        
        var state = await redisService.GetAllFieldsAsync(correlationId.ToString());

        if (state.Count == 0)
        {
            return false;
        }
        
        var isCompleted = await redisService.GetFieldValueAsync<bool>(completedKey, "isCompleted");

        if (isCompleted)
        {
            return false;
        }

        if (state.SetEquals(coordinator.RequiredServices))
        {
            return false;
        }
        
        await redisService.SetFieldAsync(completedKey, "isCompleted", true, TimeSpan.FromMinutes(5));
        await onFailure();
        await redisService.RemoveAsync(correlationId.ToString());
        await redisService.ReleaseLockAsync(lockKey);
        
        return true;
    }
}