using Common.Domain.Models.Results;
using MediatR;
using ProductsService.Domain.Entities;
using ProductsService.Domain.Interfaces;

namespace ProductsService.Application.Features.Categories.Commands.AddCategoryToProduct;

public class AddCategoryCommandHandler(
    IProductsRepository productsRepository,
    ICategoriesRepository categoriesRepository) : IRequestHandler<AddCategoryCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(AddCategoryCommand request, CancellationToken cancellationToken)
    {
        var product = await productsRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            return ApiResponse.NotFound(nameof(Product));
        }
        
        var category = await categoriesRepository.GetByIdAsync(request.CategoryId, cancellationToken);

        if (category is null)
        {
            return ApiResponse.NotFound(nameof(Category));
        }
        
        product.Categories.Add(category);
        await productsRepository.SaveChangesAsync(cancellationToken);

        return ApiResponse.Ok();
    }
}