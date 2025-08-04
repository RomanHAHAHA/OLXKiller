using System.Collections.Concurrent;
using Common.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Common.Application.Services;

public class CacheService<T>(
    IConnectionMultiplexer connectionMultiplexer,
    ILogger<CacheService<T>> logger) : ICacheService<T>
{
    private readonly IDatabase _db = connectionMultiplexer.GetDatabase();
    private static readonly ConcurrentDictionary<string, bool> CacheKeys = new();

    public async Task SetAsync(
        string key,
        T data,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        var serializedData = JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        });

        await _db.StringSetAsync(key, serializedData, expiration);
        CacheKeys.TryAdd(key, false);

        //logger.LogInformation($"Cache [{key}] set.");
    }

    public async Task<bool> SetIfNotExistsAsync(string key, T data, TimeSpan expiration)
    {
        var serializedData = JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        });

        var result = await _db.StringSetAsync(key, serializedData, expiration, When.NotExists);
        // logger.LogInformation(result
        //     ? $"Cache [{key}] set with NX."
        //     : $"Cache [{key}] already exists.");

        if (result)
        {
            CacheKeys.TryAdd(key, false);
        }
        
        return result;
    }

    public async Task<T?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        var value = await _db.StringGetAsync(key);
        if (value.IsNullOrEmpty)
        {
            //logger.LogInformation($"Cache miss for key [{key}].");
            return default;
        }

        //logger.LogInformation($"Cache return value with key [{key}].");
        return JsonConvert.DeserializeObject<T>(value!);
    }

    public async Task<T?> GetAsync(
        string key,
        Func<Task<T?>> factory,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        var cachedValue = await GetAsync(key, cancellationToken);
        
        if (cachedValue is not null)
        {
            return cachedValue;
        }

        var newValue = await factory();
        
        if (newValue is null)
        {
            return default;
        }

        await SetAsync(key, newValue, expiration, cancellationToken);
        
        return newValue;
    }

    public async Task UpdateAsync(
        string key, 
        Func<T?, Task<T>> updateFunc, 
        TimeSpan expiration, 
        CancellationToken token)
    {
        var value = await GetAsync(key, token);
        var updated = await updateFunc(value);
        
        await SetAsync(key, updated, expiration, token);
    }
    
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _db.KeyDeleteAsync(key);
        CacheKeys.TryRemove(key, out _);

        //logger.LogInformation($"Cache [{key}] removed.");
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        var keys = CacheKeys.Keys.Where(k => k.StartsWith(prefix)).ToList();
        var tasks = keys.Select(k => RemoveAsync(k, cancellationToken));
        await Task.WhenAll(tasks);

        //logger.LogInformation($"All cache keys with prefix [{prefix}] removed.");
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        var exists = await _db.KeyExistsAsync(key);
        //logger.LogInformation($"Cache [{key}] exists: {exists}");
        return exists;
    }
}