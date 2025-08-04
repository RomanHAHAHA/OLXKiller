using Common.Domain.Abstractions;

namespace Common.Infrastructure.Messaging.Events.Product;

public class ProductSnapshotUpdatedEvent : BaseEvent
{
    public required Guid ProductId { get; init; }
    
    public required Guid UserId { get; init; }
}