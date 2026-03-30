using System.Linq.Expressions;
using OrdersService.Domain.Dtos;
using OrdersService.Domain.Entities;

namespace OrdersService.Application.Features.Orders.Queries.GetOrderDetails;

public class OrderDetailsDto
{
    public required DeliveryLocationDto DeliveryLocation { get; init; }
    
    public required List<OrderItemDto> OrderItems { get; init; }
    
    public required List<OrderStatusDto> StatusesHistory { get; init; }
    
    public static Expression<Func<Order, OrderDetailsDto>> Projection => 
        order => new OrderDetailsDto
        {
            DeliveryLocation = new DeliveryLocationDto
            {
                City = order.DeliveryLocation!.City,
                Region = order.DeliveryLocation!.Region,
                Warehouse = order.DeliveryLocation!.Warehouse,
            },
            OrderItems = order.OrderItems.Select(oi => new OrderItemDto
            {
                ProductId = oi.ProductId,
                Name = oi.Product!.Name,
                MainImagePath = oi.Product!.MainImagePath,
                FixedPrice = oi.FixedPrice,
                Quantity = oi.Quantity
            }).ToList(),
            StatusesHistory = order.Statuses.Select(s => new OrderStatusDto
            {
                Status = s.Status.ToString(),
                CreatedAt = $"{s.CreatedAt.ToLocalTime():dd.MM.yyyy HH:mm}"
            }).ToList()
        };
}