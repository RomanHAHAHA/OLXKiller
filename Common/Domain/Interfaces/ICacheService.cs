namespace Common.Domain.Interfaces;

public interface ICacheService<T>
{
    Task SetAsync(
        string key, 
        T data, 
        TimeSpan expiration, 
        CancellationToken cancellationToken = default);

    Task<bool> SetIfNotExistsAsync(string key, T data, TimeSpan expiration);
    
    Task<T?> GetAsync(
        string key, 
        CancellationToken cancellationToken = default);
    
    Task<T?> GetAsync(
        string key, 
        Func<Task<T?>> factory, 
        TimeSpan expiration, 
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        string key,
        Func<T?, Task<T>> updateFunc,
        TimeSpan expiration,
        CancellationToken token);
    
    Task RemoveAsync(
        string key, 
        CancellationToken cancellationToken = default);
    
    Task RemoveByPrefixAsync(
        string prefix, 
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        string key, 
        CancellationToken cancellationToken = default);
}