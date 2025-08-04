using Common.Domain.Abstractions;

namespace Common.Infrastructure.Messaging.Events.Order;

public class OrderProcessedEvent : BaseEvent
{
    public required Guid UserId { get; init; } 
}
