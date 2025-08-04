using Common.Domain.Abstractions;

namespace Common.Infrastructure.Messaging.Events.CartItem;

public class ProductCartQuantityIncrementedEvent : BaseEvent
{
    public required Guid UserId { get; init; }
    
    public required Guid ProductId { get; init; }

    public required int RequestedQuantity { get; init; }
}