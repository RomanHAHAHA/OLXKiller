namespace OLXKiller.Domain.Entities;

public class ProductImageEntity
{
    public Guid Id { get; set; }

    public byte[] Data { get; set; } = [];

    public Guid ProductId { get; set; }

    public ProductEntity? Product { get; set; }

    private ProductImageEntity() { }

    public ProductImageEntity(IEnumerable<byte> data, Guid productEntity)
    {
        Id = Guid.NewGuid();
        Data = data.ToArray();
        ProductId = productEntity;
    }
}
