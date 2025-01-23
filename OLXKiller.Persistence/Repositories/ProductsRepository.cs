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
            .AsSplitQuery()
            .Include(p => p.Images)
            .Include(p => p.UsersWhoLiked)
            .Filter(productFilter)
            .Sort(sortParams)
            .ToPagedAsync(pageParams);
    }

    public async Task<ProductEntity?> GetByIdWithLikes(Guid productId)
    {
        return await _appDbContext.Products
            .Include(p => p.UsersWhoLiked)
            .FirstOrDefaultAsync(p => p.Id == productId);
    }

    public override Task RemoveAsync(ProductEntity entity)
    {
        var likesToRemove = _appDbContext.Likes
          .Where(like => like.ProductId == entity.Id);

        _appDbContext.Likes.RemoveRange(likesToRemove);

        return base.RemoveAsync(entity);
    }
}
