
using CartsService.Domain.Entities;

namespace CartsService.Domain.Interfaces;

public interface IProductsRepository
{
    Task CreateAsync(ProductSnapshot product, CancellationToken cancellationToken = default);
    
    Task<ProductSnapshot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    void Delete(ProductSnapshot product);
    
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
}