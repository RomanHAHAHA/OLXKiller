using Common.Domain.Abstractions;

namespace Common.Infrastructure.Messaging.Events.Product;

public class ProductsReservationFailedEvent : BaseEvent
{
    public required Guid OrderId { get; init; }
    
    public required Guid UserId { get; init; }
    
    public required List<ProductStockInfo> ProductStockInfos { get; init; }
}

public record ProductStockInfo(Guid ProductId, int StockQuantity);
