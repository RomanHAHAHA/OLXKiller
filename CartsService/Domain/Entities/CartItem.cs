namespace CartsService.Domain.Entities;

public class CartItem
{
    public Guid ProductId { get; set; }

    public ProductSnapshot? ProductSnapshot { get; set; }
    
    public Guid UserId { get; set; }

    public int Quantity { get; set; }
}