using Common.Domain.Abstractions;
using Common.Domain.Enums;

namespace OrdersService.Domain.Entities;

public class OrderStatusHistoryItem : Entity<Guid>
{
    public Guid OrderId { get; set; }

    public Order? Order { get; set; }
    
    public OrderStatus Status { get; set; }
}