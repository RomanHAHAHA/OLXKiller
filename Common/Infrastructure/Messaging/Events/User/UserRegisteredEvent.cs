using Common.Domain.Abstractions;

namespace Common.Infrastructure.Messaging.Events.User;

public class UserRegisteredEvent : BaseEvent
{
    public required Guid UserId { get; init; } 
    
    public required string NickName { get; init; }
    
    public required string Email { get; init; } 
    
    public required DateTime RegisterDate { get; init; }
    
    public required string ConnectionId { get; init; } 
}