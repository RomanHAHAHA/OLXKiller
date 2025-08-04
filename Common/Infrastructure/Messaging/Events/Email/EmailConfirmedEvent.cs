using Common.Domain.Abstractions;

namespace Common.Infrastructure.Messaging.Events.Email;

public class EmailConfirmedEvent : BaseEvent
{
    public required string Email { get; init; }
}