using Common.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using ProductsService.Domain.Entities;
using ProductsService.Domain.Interfaces;

namespace ProductsService.Infrastructure.Persistence.Repositories;

public class CategoriesRepository(ProductsDbContext dbContext) : 
    Repository<ProductsDbContext, Category, Guid>(dbContext),
    ICategoriesRepository
{
    public async Task<CategoryWithSubFlagDto?> GetCategoryWithSubFlagAsync(
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Categories
            .AsNoTracking()
            .Where(c => c.Id == categoryId)
            .Select(c => new CategoryWithSubFlagDto
            {
                Id = c.Id,
                HasSubCategories = dbContext.Categories.Count(sub => sub.ParentCategoryId == c.Id) > 0,
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}

public class CategoryWithSubFlagDto
{
    public Guid Id { get; set; }

    public bool HasSubCategories { get; set; }
}