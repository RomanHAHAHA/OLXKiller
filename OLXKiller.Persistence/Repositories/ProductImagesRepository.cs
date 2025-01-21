using OLXKiller.Domain.Abstractions.Repositories;
using OLXKiller.Domain.Entities;
using OLXKiller.Persistence.Contexts;

namespace OLXKiller.Persistence.Repositories;

public class ProductImagesRepository(AppDbContext appDbContext) :
    Repository<ProductImageEntity>(appDbContext),
    IProductImagesRepository
{
    public async Task CreateRange(IEnumerable<ProductImageEntity> images)
    {
        await _appDbContext.AddRangeAsync(images);
        await _appDbContext.SaveChangesAsync();
    }
}
