using System.Data;
using Common.Domain.Abstractions;
using Common.Domain.Dtos;
using Common.Domain.Extensions;
using Common.Domain.Models.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ProductsService.Application.Features.Products.Queries.GetPagedList;
using ProductsService.Domain.Entities;
using ProductsService.Domain.Interfaces;

namespace ProductsService.Infrastructure.Persistence.Repositories;

public class ProductsRepository(ProductsDbContext dbContext) : 
    Repository<ProductsDbContext, Product, Guid>(dbContext),
    IProductsRepository
{
    public async Task<IDbContextTransaction> BeginTransactionAsync(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default)
    {
        return await AppDbContext.Database
            .BeginTransactionAsync(isolationLevel, cancellationToken);
    }
    
    public async Task<PagedList<Product>> GetProductsAsync(
        ProductFilter productFilter,
        SortParams sortParams,
        PageParams pageParams,
        CancellationToken cancellationToken = default)
    {
        return await AppDbContext.Products
            .AsNoTracking()
            .AsSplitQuery()
            .Include(p => p.Categories)
            .Include(p => p.Images)
            .Filter(productFilter)
            .Sort(sortParams)
            .ToPagedAsync(pageParams, cancellationToken);
    }

    public async Task<Product?> GetAllInfoByIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        return await AppDbContext.Products
            .AsNoTracking()
            .AsSplitQuery()
            .Include(p => p.User)
            .Include(p => p.Characteristics)
            .Include(p => p.Categories)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
    }

    public async Task<Product?> GetByIdWithImagesAsync(
        Guid productId, 
        CancellationToken cancellationToken = default)
    {
        return await AppDbContext.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
    }

    public async Task<Product?> GetByIdWithCategoriesAsync(
        Guid productId, 
        CancellationToken cancellationToken = default)
    {
        return await AppDbContext.Products
            .Include(p => p.Categories)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
    }
    
    public async Task<Product?> GetByIdWithCharacteristicsAsync(
        Guid productId, 
        CancellationToken cancellationToken = default)
    {
        return await AppDbContext.Products
            .Include(p => p.Characteristics)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
    }

    public async Task<int?> GetQuantityById(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var product = await AppDbContext.Products
            .AsNoTracking()
            .Where(p => p.Id == productId)
            .FirstOrDefaultAsync(cancellationToken);

        return product?.StockQuantity;
    }

    public async Task<List<Product>> GetProductsByIdsAsync(
        List<Guid> productIds, 
        CancellationToken cancellationToken = default)
    {
        return await AppDbContext.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }
}