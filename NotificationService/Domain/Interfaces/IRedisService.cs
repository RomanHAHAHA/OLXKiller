namespace NotificationService.Domain.Interfaces;

public interface IRedisService
{
    // Hash методы для string (без значения)
    Task SetFieldAsync(string key, string field, TimeSpan expiration);
    
    Task SetFieldAsync<T>(string key, string field, T value, TimeSpan expiration);
    
    // Hash методы для чтения
    Task<HashSet<string>> GetAllFieldsAsync(string key);
    
    Task<T?> GetFieldValueAsync<T>(string key, string field);
    
    // Hash удаление
    Task RemoveAsync(string key);
    
    Task RemoveFieldAsync(string key, string field);
    
    // Lock методы
    Task<bool> AcquireLockAsync(string key, TimeSpan expiry, int maxRetries, TimeSpan delayBetweenRetries, CancellationToken cancellationToken = default);
    
    Task ReleaseLockAsync(string key);
}