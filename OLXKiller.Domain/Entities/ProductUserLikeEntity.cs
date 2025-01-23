namespace OLXKiller.Domain.Entities;

public class ProductUserLikeEntity
{
    public Guid ProductId { get; set; }

    public ProductEntity? Product { get; set; } 

    public Guid UserId { get; set; }

    public UserEntity? User { get; set; }

    private ProductUserLikeEntity() { }

    public ProductUserLikeEntity(Guid productId, Guid userId)
    {
        ProductId = productId;
        UserId = userId;
    }
}
