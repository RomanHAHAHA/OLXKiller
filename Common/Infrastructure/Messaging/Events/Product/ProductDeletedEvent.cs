using Common.Domain.Abstractions;

namespace Common.Infrastructure.Messaging.Events.Product;

public class ProductDeletedEvent : BaseEvent
{
    public required Guid ProductId { get; init; } 
}