using Common.Domain.Abstractions;

namespace Common.Infrastructure.Messaging.Events.User;

public class UserAvatarUpdatedEvent : BaseEvent
{
    public required Guid UserId { get; init; } 

    public required string AvatarPath { get; init; }
}