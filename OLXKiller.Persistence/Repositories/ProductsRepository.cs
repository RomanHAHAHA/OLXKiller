using Microsoft.EntityFrameworkCore;
using OLXKiller.Domain.Abstractions.Repositories;
using OLXKiller.Domain.Entities;
using OLXKiller.Domain.Models;
using OLXKiller.Persistence.Contexts;
using OLXKiller.Persistence.Extensions;

namespace OLXKiller.Persistence.Repositories;

public class ProductsRepository(AppDbContext appDbContext) :
    Repository<ProductEntity>(appDbContext),
    IProductsRepository
{
    public async Task<PagedResult<ProductEntity>> GetPaigedCollectionAsync(
        ProductFilter productFilter,
        ProductSortParams sortParams,
        PageParams pageParams)
    {
        return await GetAllAsync()
            .AsNoTracking()
            .Include(p => p.Images)
            .Filter(productFilter)
            .Sort(sortParams)
            .ToPagedAsync(pageParams);
    }
}
