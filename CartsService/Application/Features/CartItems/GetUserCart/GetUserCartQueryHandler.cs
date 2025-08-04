using CartsService.Domain.Interfaces;
using Common.Domain.Dtos;
using Common.Domain.Entities;
using MediatR;

namespace CartsService.Application.Features.CartItems.GetUserCart;

public class GetUserCartQueryHandler(
    ICartsRepository cartsRepository) : IRequestHandler<GetUserCartQuery, CartDto>
{
    public async Task<CartDto> Handle(GetUserCartQuery request, CancellationToken cancellationToken)
    {
        var cartItems = await cartsRepository
            .GetUserCartByIdAsync(request.UserId, cancellationToken);

        return new CartDto
        {
            UserId = request.UserId,
            CartItems = cartItems
                .Select(ci => new CartItemDto
                {
                    Product = new ProductSnapshot
                    {
                        Id = ci.ProductId,
                        Name = ci.ProductSnapshot!.Name,
                        Price = ci.ProductSnapshot!.Price,
                        MainImagePath = ci.ProductSnapshot!.MainImagePath,
                    },
                    Quantity = ci.Quantity
                }).ToList(),
        };;
    }
}