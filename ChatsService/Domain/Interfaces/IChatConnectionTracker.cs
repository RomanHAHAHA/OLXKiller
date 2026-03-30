namespace ChatsService.Domain.Interfaces;

public interface IChatConnectionTracker
{
    Task SetConnectionAsync(Guid userId, string connectionId, Guid chatId);
    
    Task FullRemoveConnectionAsync(string connectionId);

    Task<bool> IsUserInChatAsync(Guid userId, Guid chatId);
}