namespace OrdersService.Domain.Dtos;

public class OrderStatusDto
{
    public required string Status { get; init; }

    public required string CreatedAt { get; init; }
}