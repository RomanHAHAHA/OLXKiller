namespace NotificationService.Domain.Interfaces;

public interface IRedisLockService
{
    Task<bool> TryAcquireLockAsync(
        string key, 
        TimeSpan expiry, 
        CancellationToken cancellationToken = default);

    Task<bool> AcquireLockWithRetryAsync(
        string key,
        TimeSpan expiry,
        int maxRetries,
        TimeSpan delayBetweenRetries,
        CancellationToken cancellationToken = default);
    
    Task ReleaseLockAsync(string key);
}