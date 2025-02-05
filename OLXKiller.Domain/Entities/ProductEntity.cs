namespace OLXKiller.Domain.Entities;

public class ProductEntity 
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Amount { get; set; }

    public ICollection<ProductImageEntity> Images { get; set; } = [];

    public ICollection<UserEntity> UsersWhoLiked { get; set; } = [];
}
