using Common.Domain.Abstractions;

namespace Common.Infrastructure.Messaging.Events.Product;

public class ProductStockExceededEvent : BaseEvent
{
    public required Guid UserId { get; init; }
    
    public required int StockQuantity { get; init; }
}