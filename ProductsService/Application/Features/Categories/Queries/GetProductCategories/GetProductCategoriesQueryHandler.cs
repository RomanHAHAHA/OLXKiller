using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductsService.Domain.Dtos;
using ProductsService.Infrastructure.Persistence;

namespace ProductsService.Application.Features.Categories.Queries.GetProductCategories;

public class GetProductCategoriesQueryHandler(
    ProductsDbContext dbContext) : IRequestHandler<GetProductCategoriesQuery, List<ShortCategoryDto>>
{
    public async Task<List<ShortCategoryDto>> Handle(
        GetProductCategoriesQuery request, 
        CancellationToken cancellationToken)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Include(p => p.Categories)
            .Where(p => p.Id == request.ProductId)
            .SelectMany(p => p.Categories.Select(c => new ShortCategoryDto(c.Id, c.Name)))
            .ToListAsync(cancellationToken);
    }
}