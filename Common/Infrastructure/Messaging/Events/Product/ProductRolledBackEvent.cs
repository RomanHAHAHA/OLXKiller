using Common.Domain.Abstractions;

namespace Common.Infrastructure.Messaging.Events.Product;

public class ProductRolledBackEvent : BaseEvent
{
    public required Guid Id { get; set; }
    
    public required string Name { get; set; } = string.Empty;

    public required decimal Price { get; set; }
}