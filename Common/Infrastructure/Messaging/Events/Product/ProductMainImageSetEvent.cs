using Common.Domain.Abstractions;

namespace Common.Infrastructure.Messaging.Events.Product;

public class ProductMainImageSetEvent : BaseEvent
{
    public required Guid ProductId { get; init; } 
    
    public required string ImagePath { get; init; }
}