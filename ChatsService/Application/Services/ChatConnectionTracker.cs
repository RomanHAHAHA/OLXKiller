using System.Text.Json;
using ChatsService.Domain.Interfaces;
using ChatsService.Domain.Models;
using ChatsService.Infrastructure.Persistence;
using StackExchange.Redis;

namespace ChatsService.Application.Services;

public class ChatConnectionTracker(
    IConnectionMultiplexer connection,
    ChatsDbContext dbContext,
    ILogger<ChatConnectionTracker> logger) : IChatConnectionTracker
{
    private readonly IDatabase _redisDb = connection.GetDatabase();

    public async Task SetConnectionAsync(Guid userId, string connectionId, Guid chatId)
    {
        try
        {
            var user = await dbContext.UserSnapshots.FindAsync(userId);
            
            var data = new UserConnection
            {
                ConnectionId = connectionId,
                CurrentChatId = chatId,
                NickName = user?.NickName ?? string.Empty,
                AvatarPath = user?.AvatarImageName ?? string.Empty,
            };

            await _redisDb.StringSetAsync($"user:{userId}", JsonSerializer.Serialize(data));
            logger.LogInformation($"{data.NickName} CurrentChatId: {data.CurrentChatId}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error setting connection for user {userId}");
            throw;
        }
    }

    public async Task FullRemoveConnectionAsync(string connectionId)
    {
        try
        {
            var endpoints = connection.GetEndPoints();
            
            foreach (var endpoint in endpoints)
            {
                var server = connection.GetServer(endpoint);
                await foreach (var key in server.KeysAsync(pattern: "user:*"))
                {
                    try
                    {
                        var json = await _redisDb.StringGetAsync(key);

                        if (json.IsNullOrEmpty)
                        {
                            continue;
                        }
                        
                        var signalRConnection = JsonSerializer.Deserialize<UserConnection>(json!);
                        
                        if (signalRConnection?.ConnectionId == connectionId)
                        {
                            var transaction = _redisDb.CreateTransaction();
                            transaction.AddCondition(Condition.StringEqual(key, json));
                            _ = transaction.KeyDeleteAsync(key);
                            
                            if (await transaction.ExecuteAsync())
                            {
                                logger.LogInformation($"Removed connection {connectionId} for key {key}");
                                return;
                            }
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        logger.LogWarning(jsonEx, $"Invalid JSON data in Redis key {key}");
                    }
                    catch (RedisException redisEx)
                    {
                        logger.LogWarning(redisEx, $"Redis error processing key {key}");
                    }
                }
            }
            
            logger.LogWarning($"Connection {connectionId} not found in tracker");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error in FullRemoveConnectionAsync for {connectionId}");
            throw;
        }
    }
    
    public async Task<bool> IsUserInChatAsync(Guid userId, Guid chatId)
    {
        try
        {
            var data = await GetUserData(userId);
            
            if (data is null)
            {
                return false;
            }
            
            logger.LogDebug($"{data.NickName} CurrentChatId: {data.CurrentChatId}");
            return data.CurrentChatId == chatId;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error checking if user {userId} is in chat {chatId}");
            return false;
        }
    }
    
    private async Task<UserConnection?> GetUserData(Guid userId)
    {
        try
        {
            var json = await _redisDb.StringGetAsync($"user:{userId}");

            if (json.IsNullOrEmpty)
            {
                logger.LogDebug($"No data found for user {userId}");
                return null;
            }
        
            return JsonSerializer.Deserialize<UserConnection>(json!);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, $"Invalid JSON data for user {userId}");
            return null;
        }
        catch (RedisException ex)
        {
            logger.LogError(ex, $"Redis error getting data for user {userId}");
            return null;
        }
    }
}