using Common.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using OrdersService.Domain.Interfaces;

namespace OrdersService.Infrastructure.Persistence.Repositories;

public class ProductsesRepository(OrdersDbContext dbContext) : IProductsRepository
{
    public async Task CreateAsync(
        ProductSnapshot product, 
        CancellationToken cancellationToken = default)
    {
        await dbContext.Product.AddAsync(product, cancellationToken); 
    }

    public async Task<ProductSnapshot?> GetByIdAsync(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Product
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public void Delete(ProductSnapshot product) => dbContext.Product.Remove(product);

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default) 
        => await dbContext.SaveChangesAsync(cancellationToken) > 0;

    public async Task<List<ProductSnapshot>> GetWithIdsAsync(
        List<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Product
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }
}