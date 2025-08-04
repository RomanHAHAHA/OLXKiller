using Common.Domain.Abstractions;

namespace Common.Infrastructure.Messaging.Events.Product;

public class ProductsReservedEvent : BaseEvent
{
    public required Guid OrderId { get; init; } 
    
    public required Guid UserId { get; init; } 
}