namespace OLXKiller.Domain.Entities;

public class CartItemEntity
{
    public int Quantity { get; set; } 

    public Guid ProductId { get; set; }

    public ProductEntity? Product { get; set; }

    public Guid UserId { get; set; }

    public UserEntity? User { get; set; }
}
