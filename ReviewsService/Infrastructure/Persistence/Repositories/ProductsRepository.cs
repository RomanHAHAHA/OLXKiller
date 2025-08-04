using Microsoft.EntityFrameworkCore;
using ReviewsService.Domain.Entities;
using ReviewsService.Domain.Interfaces;

namespace ReviewsService.Infrastructure.Persistence.Repositories;

public class ProductsRepository(ReviewsDbContext dbContext) : IProductsRepository
{
    public async Task CreateAsync(
        ProductSnapshot product,
        CancellationToken cancellationToken = default)
    {
        await dbContext.ProductSnapshots.AddAsync(product, cancellationToken);
    }

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await dbContext.SaveChangesAsync(cancellationToken) > 0;
    
    public async Task<ProductSnapshot?> GetByIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.ProductSnapshots
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.ProductSnapshots
            .AnyAsync(p => p.Id == productId, cancellationToken);
    }

    public void Delete(ProductSnapshot product) => dbContext.ProductSnapshots.Remove(product);
}