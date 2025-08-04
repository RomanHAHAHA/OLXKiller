using Common.Domain.Abstractions;

namespace Common.Infrastructure.Messaging.Events.Product;

public class ProductCreatedEvent : BaseEvent
{
    public required Guid ProductId { get; init; }
    public required Guid SellerId { get; init; }
    public required string Name { get; init; }
    public required decimal Price { get; init; } 
}