using System.Data;
using Common.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using ProductsService.Domain.Entities;

namespace ProductsService.Domain.Interfaces;

public interface IProductImagesRepository : IRepository<ProductImage, Guid>
{
    Task<List<ProductImage>> GetProductImagesAsync(
        Guid productId,
        CancellationToken cancellationToken = default);
    
    Task<IDbContextTransaction> BeginTransactionAsync(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default);
}