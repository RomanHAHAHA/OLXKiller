using System.Data;
using Common.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ProductsService.Domain.Entities;
using ProductsService.Domain.Interfaces;

namespace ProductsService.Infrastructure.Persistence.Repositories;

public class ProductImagesRepository(ProductsDbContext dbContext) : 
    Repository<ProductsDbContext, ProductImage, Guid>(dbContext),
    IProductImagesRepository
{
    public async Task<List<ProductImage>> GetProductImagesAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        return await AppDbContext.ProductImages
            .Where(p => p.ProductId == productId)
            .OrderBy(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default)
    {
        return await AppDbContext.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
    }
}