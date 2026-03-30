using OrdersService.Domain.Entities;

namespace OrdersService.Domain.Dtos;

public class ShortOrdersDto
{
    public required Guid Id { get; init; }

    public required string CreatedAt { get; init; } 
    
    public required UserDto User { get; init; }
    
    public required string LastStatus { get; init; }
    
    public required decimal TotalPrice { get; init; }

    public static ShortOrdersDto FromEntity(Order order)
    {
        return new ShortOrdersDto
        {
            Id = order.Id,
            CreatedAt = $"{order.CreatedAt.ToLocalTime():dd.MM.yyyy HH:mm}",
            LastStatus = order.Statuses
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => s.Status.ToString())
                .FirstOrDefault() ?? "Unknown",
            User = new UserDto
            {
                Id = order.UserId,
                NickName = order.User!.NickName,
                AvatarName = order.User.AvatarPath
            },
            TotalPrice = order.OrderItems.Sum(oi => oi.FixedPrice * oi.Quantity)
        };
    }
}