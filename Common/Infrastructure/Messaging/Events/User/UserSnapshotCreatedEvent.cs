using Common.Domain.Abstractions;

namespace Common.Infrastructure.Messaging.Events.User;

public class UserSnapshotCreatedEvent : BaseEvent
{
    public required Guid UserId { get; init; } 
    
    public required string ConnectionId { get; init; }
}