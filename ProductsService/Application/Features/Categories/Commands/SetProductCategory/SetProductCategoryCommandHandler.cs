using Common.Domain.Models.Results;
using MediatR;
using ProductsService.Domain.Entities;
using ProductsService.Domain.Interfaces;

namespace ProductsService.Application.Features.Categories.Commands.SetProductCategory;

public class SetProductCategoryCommandHandler(
    IProductsRepository productsRepository,
    ICategoriesRepository categoriesRepository) : IRequestHandler<SetProductCategoryCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(SetProductCategoryCommand request, CancellationToken cancellationToken)
    {
        var product = await productsRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            return ApiResponse.NotFound(nameof(Product));
        }

        if (request.CategoryId is null)
        {
            return await UpdateProductCategory(product, null, cancellationToken);
        }

        var category = await categoriesRepository
            .GetCategoryWithSubFlagAsync(request.CategoryId.Value, cancellationToken);

        if (category is null)
        {
            return ApiResponse.NotFound(nameof(Category));
        }

        if (category.HasSubCategories)
        {
            return ApiResponse.Conflict("Choose the last child category");
        }

        return await UpdateProductCategory(product, category.Id, cancellationToken);
    }

    private async Task<ApiResponse> UpdateProductCategory(
        Product product, 
        Guid? categoryId, 
        CancellationToken cancellationToken)
    {
        product.CategoryId = categoryId;
        await productsRepository.SaveChangesAsync(cancellationToken);
            
        return ApiResponse.Ok();
    }
}