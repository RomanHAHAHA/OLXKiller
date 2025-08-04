namespace ProductsService.Domain.Entities;

public class ProductCharacteristic
{
    public Guid Id { get; set; }
    
    public Guid ProductId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public Product? Product { get; set; }
}