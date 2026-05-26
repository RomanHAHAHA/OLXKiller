using ProductsService.Domain.Entities;

namespace ProductsService.Application.Features.Products.Commands.Update;

public class OldProductProperties(Product product)
{
    public Guid Id { get; set; } = product.Id;

    public string Name { get; set; } = product.Name;
    
    public string Description { get; set; } = product.Description;
    
    public decimal Price { get; set; } = product.Price;
    
    public int StockQuantity { get; set; } = product.StockQuantity;
}