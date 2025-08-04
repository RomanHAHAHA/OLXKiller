using CartsService.Domain.Entities;
using CartsService.Domain.Interfaces;
using Common.Domain.Models.Results;
using MediatR;

namespace CartsService.Application.Features.CartItems.Delete;

public class DeleteItemCommandHandler(
    ICartsRepository cartsRepository) : IRequestHandler<DeleteItemCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(DeleteItemCommand request, CancellationToken cancellationToken)
    {
        var cartItem = await cartsRepository.GetByIdAsync(
            request.UserId,
            request.ProductId,
            cancellationToken);

        if (cartItem is null)
        {
            return ApiResponse.NotFound(nameof(CartItem));
        }

        cartsRepository.Delete(cartItem);
        await cartsRepository.SaveChangesAsync(cancellationToken);
        
        return ApiResponse.Ok();
    }
}