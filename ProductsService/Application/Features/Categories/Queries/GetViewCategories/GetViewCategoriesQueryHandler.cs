using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductsService.Domain.Entities;
using ProductsService.Infrastructure.Persistence;

namespace ProductsService.Application.Features.Categories.Queries.GetViewCategories;

public class GetViewCategoriesQueryHandler(
    ProductsDbContext dbContext) : IRequestHandler<GetViewCategoriesQuery, List<CategoryNodeDto>>
{
    public async Task<List<CategoryNodeDto>> Handle(
        GetViewCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var categories = await dbContext.Categories
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var productsCount = await dbContext.Products
            .Where(p => p.CategoryId != null)
            .GroupBy(p => p.CategoryId)
            .Select(g => new
            {
                CategoryId = g.Key!.Value,
                Count = g.Count()
            })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Count, cancellationToken);

        var lookup = categories.ToLookup(c => c.ParentCategoryId);

        return BuildTree(null);

        List<CategoryNodeDto> BuildTree(Guid? parentId)
        {
            return lookup[parentId]
                .Select(BuildNode)
                .ToList();
        }

        CategoryNodeDto BuildNode(Category category) 
        {
            var children = BuildTree(category.Id);

            var ownCount = productsCount.GetValueOrDefault(category.Id, 0);
            var childrenCount = children.Sum(x => x.ProductCount);

            return new CategoryNodeDto
            {
                Id = category.Id,
                Name = category.Name,
                Children = children,
                ProductCount = ownCount + childrenCount
            };
        }
    }
}