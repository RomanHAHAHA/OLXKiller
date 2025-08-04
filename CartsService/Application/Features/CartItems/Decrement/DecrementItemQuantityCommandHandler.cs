using CartsService.Domain.Entities;
using CartsService.Domain.Interfaces;
using Common.Domain.Models.Results;
using MediatR;

namespace CartsService.Application.Features.CartItems.Decrement;

public class DecrementItemQuantityCommandHandler(
    ICartsRepository cartsRepository) : IRequestHandler<DecrementItemQuantityCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(
        DecrementItemQuantityCommand request, 
        CancellationToken cancellationToken)
    {
        var cartItem = await cartsRepository.GetByIdAsync(
            request.UserId,
            request.ProductId,
            cancellationToken);

        if (cartItem is null)
        {
            return ApiResponse.NotFound(nameof(CartItem));
        }

        if (cartItem.Quantity == 1)
        {
            return ApiResponse.Conflict("Can`t decrement item quantity because it is already 1.");
        }
        
        cartItem.Quantity--;
        await cartsRepository.SaveChangesAsync(cancellationToken);
        
        return ApiResponse.Ok();
    }
}