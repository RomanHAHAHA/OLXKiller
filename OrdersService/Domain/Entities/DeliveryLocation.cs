namespace OrdersService.Domain.Entities;

public class DeliveryLocation
{
    public Guid OrderId { get; set; }
    
    public string Region { get; set; } = string.Empty;
    
    public string City { get; set; } = string.Empty;

    public string Warehouse { get; set; } = string.Empty;
}