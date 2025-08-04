namespace ChatsService.Domain.Entities;

public class UserMute
{
    public Guid MutingUserId { get; set; }           
    
    public UserSnapshot? MutingUser { get; set; }
    
    public Guid MutedUserId { get; set; }            

    public UserSnapshot? MutedUser { get; set; }
}