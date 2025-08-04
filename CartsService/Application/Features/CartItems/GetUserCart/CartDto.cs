using Common.Domain.Dtos;

namespace CartsService.Application.Features.CartItems.GetUserCart;

public class CartDto
{
    public Guid UserId { get; set; }

    public List<CartItemDto> CartItems { get; set; } = [];

    public decimal TotalCartPrice => CartItems.Sum(ci => ci.TotalPrice);
}