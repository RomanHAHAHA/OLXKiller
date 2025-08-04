using Common.Domain.Models.Results;
using MediatR;
using ProductsService.Domain.Entities;
using ProductsService.Domain.Interfaces;

namespace ProductsService.Application.Features.Categories.Commands.RemoveCategoryFromProduct;

public class RemoveCategoryCommandHandler(
    IProductsRepository productsRepository) : IRequestHandler<RemoveCategoryCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(RemoveCategoryCommand request, CancellationToken cancellationToken)
    {
        var product = await productsRepository
            .GetByIdWithCategoriesAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            return ApiResponse.NotFound(nameof(Product));
        }    
        
        product.Categories = product.Categories
            .Where(c => c.Id != request.CategoryId)
            .ToList();
        
        await productsRepository.SaveChangesAsync(cancellationToken);

        return ApiResponse.Ok();
    }
}