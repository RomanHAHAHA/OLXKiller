using Common.Domain.Abstractions;

namespace Common.Infrastructure.Messaging.Events.User;

public class UserAvatarRollbackEvent : BaseEvent
{
    public required Guid UserId { get; init; }
    
    public required string PreviousAvatarName { get; init; }
}