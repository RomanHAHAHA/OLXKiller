using Common.Domain.Abstractions;
using ProductsService.Domain.Entities;
using ProductsService.Domain.Interfaces;

namespace ProductsService.Infrastructure.Persistence.Repositories;

public class CategoriesRepository(ProductsDbContext dbContext) : 
    Repository<ProductsDbContext, Category, Guid>(dbContext),
    ICategoriesRepository
{
}

