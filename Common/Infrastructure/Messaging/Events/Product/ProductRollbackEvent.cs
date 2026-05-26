using Common.Domain.Abstractions;

namespace Common.Infrastructure.Messaging.Events.Product;

public class ProductRollbackEvent : BaseEvent
{
    public required Guid ProductId { get; set; }
}