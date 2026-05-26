using Common.Domain.Abstractions;

namespace Common.Infrastructure.Messaging.Events.User;

public class VerifyUserRegisteredEvent : BaseEvent
{
    public required Guid UserId { get; set; }
    
    public required string ConnectionId { get; init; }
}