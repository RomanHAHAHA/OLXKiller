using Common.Domain.Abstractions;

namespace ChatsService.Domain.Entities;

public class Message : Entity<Guid>
{
    public Guid ChatId { get; set; }
    
    public Guid SenderId { get; set; }
    
    public string Content { get; set; } = string.Empty;

    public bool IsRead { get; set; }
    
    public Chat? Chat { get; set; }
    
    public UserSnapshot? Sender { get; set; }
}