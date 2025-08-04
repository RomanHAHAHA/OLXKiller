using Common.Domain.Abstractions;

namespace Common.Infrastructure.Messaging.Events.Product;

public class ProductSnapshotMainImageSetFailedEvent : BaseEvent
{
    public required Guid ProductId { get; init; }
}