using ReviewsService.Domain.Entities;

namespace ReviewsService.Domain.Interfaces;

public interface IProductsRepository
{
    Task CreateAsync(ProductSnapshot product, CancellationToken cancellationToken = default);

    Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<ProductSnapshot?> GetByIdAsync(Guid productId, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid productId, CancellationToken cancellationToken = default);

    void Delete(ProductSnapshot product);
}