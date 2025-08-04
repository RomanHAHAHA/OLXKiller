using Common.Domain.Dtos;
using Common.Domain.Entities;

namespace OrdersService.Domain.Entities;

public class OrderItem
{
    public Guid OrderId { get; set; }

    public Order? Order { get; set; }
    
    public Guid ProductId { get; set; }

    public ProductSnapshot? Product { get; set; }

    public decimal FixedPrice { get; set; }

    public int Quantity { get; set; }

    public OrderItem() { }

    public OrderItem(Guid orderId, CartItemDto cartItem)
    {
        OrderId = orderId;
        ProductId = cartItem.Product.Id;
        FixedPrice = cartItem.Product.Price;
        Quantity = cartItem.Quantity;
    }
}