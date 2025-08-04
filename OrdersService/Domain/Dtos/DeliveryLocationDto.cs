namespace OrdersService.Domain.Dtos;

public class DeliveryLocationDto
{
    public required string Region { get; init; }
    
    public required string City { get; init; }
    
    public required string Warehouse { get; init; }
}