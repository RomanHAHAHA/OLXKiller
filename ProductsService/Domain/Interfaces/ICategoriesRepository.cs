using Common.Domain.Interfaces;
using ProductsService.Domain.Entities;
using ProductsService.Infrastructure.Persistence.Repositories;

namespace ProductsService.Domain.Interfaces;

public interface ICategoriesRepository : IRepository<Category, Guid>
{
    Task<CategoryWithSubFlagDto?> GetCategoryWithSubFlagAsync(
        Guid categoryId,
        CancellationToken cancellationToken);
}