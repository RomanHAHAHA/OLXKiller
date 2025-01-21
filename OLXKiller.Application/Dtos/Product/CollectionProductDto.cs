using OLXKiller.Domain.Entities;

namespace OLXKiller.Application.Dtos.Product;

public class CollectionProductDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Amount { get; set; }

    public bool IsAvailable => Amount > 0;

    public IEnumerable<byte[]> ImagesData { get; set; } = [];

    public CollectionProductDto(ProductEntity productEntity)
    {
        Name = productEntity.Name;
        Description = productEntity.Description;
        Price = productEntity.Price;
        Amount = productEntity.Amount;
        ImagesData = productEntity.Images.Select(i => i.Data);
    }
}
