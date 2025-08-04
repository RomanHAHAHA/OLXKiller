using Common.Domain.Entities;

namespace Common.Domain.Dtos;

public class CartItemDto
{
    public required ProductSnapshot Product { get; init; }

    public required int Quantity { get; init; }

    public decimal TotalPrice => Product.Price * Quantity;
}