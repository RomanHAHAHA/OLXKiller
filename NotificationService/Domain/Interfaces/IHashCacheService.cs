namespace NotificationService.Domain.Interfaces;

public interface IHashCacheService
{
    Task SetFieldAsync(
        string key, 
        string field, 
        TimeSpan expiration, 
        CancellationToken token = default);
    
    Task<HashSet<string>> GetAllFieldsAsync(
        string key, 
        CancellationToken cancellationToken = default);
    
    Task RemoveAsync(string key);
}