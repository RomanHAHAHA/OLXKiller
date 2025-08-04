namespace ChatsService.Domain.Entities;

public class UserSnapshot
{
    public Guid Id { get; set; }
    
    public string NickName { get; set; } = string.Empty;
    
    public string AvatarImageName { get; set; } = string.Empty;
    
    public DateTime? LastOnlineAt { get; set; }
    
    public bool IsOnline { get; set; }
    
    public List<Chat> Chats { get; set; } = [];

    public List<UserMute> MutedUsers { get; set; } = [];       
    
    public List<UserMute> MutedByUsers { get; set; } = [];
}