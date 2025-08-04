namespace OrdersService.Domain.Dtos;

public class OrderItemDto
{
    public required Guid ProductId { get; init; }
    
    public required string Name { get; init; }
    
    public required string MainImagePath { get; init; }

    public required decimal FixedPrice { get; init; }
    
    public required int Quantity { get; init; }
}