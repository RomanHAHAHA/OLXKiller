using Common.Domain.Abstractions;

namespace Common.Infrastructure.Messaging.Events.User;

public class UserLoggedInEvent : BaseEvent
{
    public required Guid UserId { get; init; }

    public required DateTime LogInTime { get; init; }
}
