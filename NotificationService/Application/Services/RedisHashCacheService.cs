using NotificationService.Domain.Interfaces;
using StackExchange.Redis;

namespace NotificationService.Application.Services;

public class RedisHashCacheService(IConnectionMultiplexer connection) : IHashCacheService
{
    private readonly IDatabase _db = connection.GetDatabase();

    public async Task SetFieldAsync(
        string key, 
        string field, 
        TimeSpan expiration, 
        CancellationToken token = default)
    {
        await _db.HashSetAsync(key, field, string.Empty);
        await _db.KeyExpireAsync(key, expiration); 
    }

    public async Task<HashSet<string>> GetAllFieldsAsync(string key, CancellationToken cancellationToken = default)
    {
        var entries = await _db.HashGetAllAsync(key);
        return entries.Select(e => e.Name.ToString()).ToHashSet();
    }

    public async Task RemoveAsync(string key)
    {
        await _db.KeyDeleteAsync(key);
    }
}