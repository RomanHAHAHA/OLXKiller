using Common.Domain.Abstractions;
using Common.Domain.Dtos;

namespace Common.Infrastructure.Messaging.Events.Order;

public class OrderCreatedEvent : BaseEvent
{
    public required Guid UserId { get; init; }
    
    public required string Email { get; init; }
    
    public required Guid OrderId { get; init; }
    
    public required List<CartItemDto> CartItems { get; init; }
}