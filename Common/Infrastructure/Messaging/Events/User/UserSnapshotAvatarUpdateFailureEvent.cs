using Common.Domain.Abstractions;

namespace Common.Infrastructure.Messaging.Events.User;

public class UserSnapshotAvatarUpdateFailureEvent : BaseEvent
{
    public required Guid UserId { get; init; }
}