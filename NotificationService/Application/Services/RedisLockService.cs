using NotificationService.Domain.Interfaces;
using StackExchange.Redis;

namespace NotificationService.Application.Services;

public class RedisLockService(IConnectionMultiplexer redis) : IRedisLockService
{
    private readonly IDatabase _db = redis.GetDatabase();

    private readonly string _lockValue = Guid.NewGuid().ToString();

    public async Task<bool> TryAcquireLockAsync(
        string key, 
        TimeSpan expiry, 
        CancellationToken cancellationToken = default)
    {
        return await _db.StringSetAsync(key, _lockValue, expiry, When.NotExists);
    }
    
    public async Task<bool> AcquireLockWithRetryAsync(
        string key, 
        TimeSpan expiry, 
        int maxRetries, 
        TimeSpan delayBetweenRetries,
        CancellationToken cancellationToken = default)
    {   
        for (var i = 0; i < maxRetries; i++)
        {
            if (await TryAcquireLockAsync(key, expiry, cancellationToken))
            {
                return true;
            }
                
            await Task.Delay(delayBetweenRetries, cancellationToken);
        }

        return false;
    }

    public async Task ReleaseLockAsync(string key)
    {
        const string luaScript = @"
            if redis.call('GET', KEYS[1]) == ARGV[1] then
                return redis.call('DEL', KEYS[1])
            else
                return 0
            end";

        await _db.ScriptEvaluateAsync(luaScript, [key], [_lockValue]);
    }
}