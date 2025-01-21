using OLXKiller.Domain.Entities;
using OLXKiller.Domain.Models;

namespace OLXKiller.Domain.Abstractions.Repositories;

public interface IProductsRepository : IRepository<ProductEntity>
{
    Task<PagedResult<ProductEntity>> GetPaigedCollectionAsync(
        ProductFilter productFilter,
        ProductSortParams sortParams,
        PageParams pageParams);
}
