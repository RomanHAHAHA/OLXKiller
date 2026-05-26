using Common.Domain.Abstractions;

namespace Common.Infrastructure.Messaging.Events.Product;

public class VerifyProductUpdatedEvent : BaseEvent
{
    public required Guid UserId { get; set; }
    
    public required Guid ProductId { get; init; } 
}