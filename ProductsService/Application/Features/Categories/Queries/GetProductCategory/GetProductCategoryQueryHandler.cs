using Common.Domain.Models.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductsService.Domain.Dtos;
using ProductsService.Domain.Entities;
using ProductsService.Infrastructure.Persistence;

namespace ProductsService.Application.Features.Categories.Queries.GetProductCategory;

public class GetProductCategoryQueryHandler(
    ProductsDbContext dbContext) : IRequestHandler<GetProductCategoryQuery, ApiResponse<ShortCategoryDto>>
{
    public async Task<ApiResponse<ShortCategoryDto>> Handle(
        GetProductCategoryQuery request, 
        CancellationToken cancellationToken)
    {
        var category = await dbContext.Products
            .AsNoTracking()
            .Where(p => p.Id == request.ProductId)
            .Select(p => p.Category == null 
                ? null 
                : new ShortCategoryDto(p.CategoryId!.Value, p.Category.Name))
            .FirstOrDefaultAsync(cancellationToken);

        return category is null
            ? ApiResponse<ShortCategoryDto>.NotFound(nameof(Category))
            : ApiResponse<ShortCategoryDto>.Ok(category);
    }
}