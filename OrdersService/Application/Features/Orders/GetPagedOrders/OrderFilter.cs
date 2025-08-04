using Common.Domain.Enums;

namespace OrdersService.Application.Features.Orders.GetPagedOrders;

public class OrderFilter
{
    public Guid? UserId { get; set; }

    public Guid? ProductId { get; set; }

    public OrderStatus? OrderStatus { get; set; }

    public decimal? MinPrice { get; set; }
    
    public DateTime? StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
}