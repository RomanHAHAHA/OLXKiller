using System.Text.Json;
using NotificationService.Domain.Interfaces;
using StackExchange.Redis;

namespace NotificationService.Application.Services;

public class RedisService(IConnectionMultiplexer connection) : IRedisService
{
    private readonly IDatabase _db = connection.GetDatabase();
    private readonly string _lockValue = Guid.NewGuid().ToString();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    
    public async Task SetFieldAsync(string key, string field, TimeSpan expiration)
    {
        await _db.HashSetAsync(key, field, string.Empty);
        await _db.KeyExpireAsync(key, expiration);
    }
    
    public async Task SetFieldAsync<T>(string key, string field, T value, TimeSpan expiration)
    {
        var jsonValue = JsonSerializer.Serialize(value, JsonOptions);
        await _db.HashSetAsync(key, field, jsonValue);
        await _db.KeyExpireAsync(key, expiration);
    }

    public async Task<HashSet<string>> GetAllFieldsAsync(string key)
    {
        var entries = await _db.HashGetAllAsync(key);
        return entries.Select(e => e.Name.ToString()).ToHashSet();
    }
    
    public async Task<T?> GetFieldValueAsync<T>(string key, string field)
    {
        var value = await _db.HashGetAsync(key, field);

        if (value.IsNullOrEmpty)
        {
            return default;
        }

        if (typeof(T) == typeof(bool))
        {
            var str = value.ToString();
        
            var result = str switch
            {
                "true" => (T)(object)true,
                "false" => (T)(object)false,
                _ => default
            };
        
            return result;
        }
    
        return JsonSerializer.Deserialize<T>(value!, JsonOptions);;
    }
    
    public async Task RemoveAsync(string key) => await _db.KeyDeleteAsync(key);
    
    public async Task RemoveFieldAsync(string key, string field) => await _db.HashDeleteAsync(key, field);
    
    
    public async Task<bool> AcquireLockAsync(
        string key, 
        TimeSpan expiry, 
        int maxRetries, 
        TimeSpan delayBetweenRetries,
        CancellationToken cancellationToken = default)
    {   
        for (var i = 0; i < maxRetries; i++)
        {
            if (await TryAcquireLockAsync(key, expiry))
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
    
    private async Task<bool> TryAcquireLockAsync(string key, TimeSpan expiry) 
        => await _db.StringSetAsync(key, _lockValue, expiry, When.NotExists);
}