using Common.Domain.Abstractions;

namespace ProductsService.Domain.Entities;

public class ProductImage : Entity<Guid>
{
    public Guid ProductId { get; set; }

    public Product? Product { get; set; }
    
    public string ImagePath { get; set; } = string.Empty;

    public bool IsMain { get; set; }
}