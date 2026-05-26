using Common.Domain.Abstractions;

namespace Common.Infrastructure.Messaging.Events.Product;

public class ProductCreationTimeoutEvent : BaseEvent
{
    public Guid UserId { get; set; }

    public Guid ProductId { get; set; }
}