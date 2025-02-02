using OLXKiller.Domain.Entities;

namespace OLXKiller.Application.Dtos.Product;

public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal? Price { get; set; }

    public int? Amount { get; set; }

    public ProductEntity AsEntity()
    {
        return new ProductEntity
        {
            Id = Guid.NewGuid(),
            Name = Name,
            Description = Description,
            Price = Price ?? decimal.Zero,
            Amount = Amount ?? 0,
        };
    }
}
