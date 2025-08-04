using OrdersService.Domain.Entities;

namespace OrdersService.Domain.Dtos;

public class OrderDto
{
    public required Guid Id { get; init; }

    public required string CreatedAt { get; init; } 
    
    public required UserDto User { get; init; } 
    
    public required List<OrderItemDto> OrderItems { get; init; } 
    
    public required DeliveryLocationDto DeliveryLocation { get; init; }
    
    public required List<OrderStatusDto> Statuses { get; init; }

    public decimal TotalPrice => OrderItems.Sum(oi => oi.FixedPrice * oi.Quantity);

    public static OrderDto FromEntity(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            CreatedAt = $"{order.CreatedAt.ToLocalTime():dd.MM.yyyy HH:mm}",
            Statuses = order.Statuses.Select(s => new OrderStatusDto
            {
                Status = s.Status.ToString(),
                CreatedAt = $"{s.CreatedAt.ToLocalTime():dd.MM.yyyy HH:mm}",
            }).ToList(),
            User = new UserDto
            {
                Id = order.UserId,
                NickName = order.User!.NickName,
                AvatarName = order.User!.AvatarPath
            },
            DeliveryLocation = new DeliveryLocationDto
            {
                Region = order.DeliveryLocation!.Region,
                City = order.DeliveryLocation!.City,
                Warehouse = order.DeliveryLocation!.Warehouse,
            },
            OrderItems = order.OrderItems.Select(oi => new OrderItemDto
            {
                ProductId = oi.ProductId,
                Name = oi.Product!.Name,
                MainImagePath = oi.Product!.MainImagePath,
                FixedPrice = oi.FixedPrice,
                Quantity = oi.Quantity,
            }).ToList()
        };
    }
}