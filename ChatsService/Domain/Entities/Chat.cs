using Common.Domain.Abstractions;

namespace ChatsService.Domain.Entities;

public class Chat : Entity<Guid>
{
    public List<UserSnapshot> Users { get; set; } = [];

    public virtual List<Message> Messages { get; set; } = [];
}