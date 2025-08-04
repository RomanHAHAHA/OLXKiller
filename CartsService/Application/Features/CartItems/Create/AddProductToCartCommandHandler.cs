using CartsService.Domain.Entities;
using CartsService.Infrastructure.Persistence;
using Common.Domain.Models.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CartsService.Application.Features.CartItems.Create;

public class AddProductToCartCommandHandler(
    CartsDbContext dbContext) : IRequestHandler<AddProductToCartCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(AddProductToCartCommand request, CancellationToken cancellationToken)
    {
        var product = await GetProductSnapshotAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            return ApiResponse.NotFound(nameof(ProductSnapshot));
        }

        if (request.UserId == product.SellerId)
        {
            return ApiResponse.BadRequest("You cannot add your own product to cart");
        }

        if (await IsProductAlreadyInCartAsync(request.ProductId, request.UserId, cancellationToken))
        {
            return ApiResponse.Conflict("Product is already in cart exists");
        }
        
        await AddProductToUserCartAsync(request.ProductId, request.UserId, cancellationToken);

        return ApiResponse.Ok();
    }

    
    private async Task<ProductSnapshot?> GetProductSnapshotAsync(
        Guid productId, 
        CancellationToken cancellationToken = default)
    {
        return await dbContext.ProductSnapshots
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == productId, cancellationToken);
    }
    
    private async Task<bool> IsProductAlreadyInCartAsync(
        Guid productId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.CartItems
            .AnyAsync(
                ci => ci.ProductId == productId && ci.UserId == userId,
                cancellationToken);
    }
    
    private async Task AddProductToUserCartAsync(
        Guid productId, 
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        var newCartItem = new CartItem
        {
            UserId = userId,
            ProductId = productId,
            Quantity = 1
        };

        await dbContext.CartItems.AddAsync(newCartItem, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}