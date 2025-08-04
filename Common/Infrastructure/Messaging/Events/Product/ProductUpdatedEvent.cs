using Common.Domain.Abstractions;

namespace Common.Infrastructure.Messaging.Events.Product;

public class ProductUpdatedEvent : BaseEvent
{
    public required Guid ProductId { get; init; } 
    
    public required Guid UserId { get; init; } 
    
    public required string Name { get; init; }
    
    public required decimal Price { get; init; } 
    
    public required int StockQuantity { get; init; }
}
