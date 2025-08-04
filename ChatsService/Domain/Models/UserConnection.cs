namespace ChatsService.Domain.Models;

public class UserConnection
{
    public string ConnectionId { get; set; } = string.Empty;
    
    public Guid CurrentChatId { get; set; }   
    
    public string NickName { get; set; } = string.Empty;
    
    public string AvatarPath { get; set; } = string.Empty;   
}