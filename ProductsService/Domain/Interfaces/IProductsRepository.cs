using System.Data;
using Common.Domain.Dtos;
using Common.Domain.Interfaces;
using Common.Domain.Models.Results;
using Microsoft.EntityFrameworkCore.Storage;
using ProductsService.Application.Features.Products.Queries.GetPagedList;
using ProductsService.Domain.Entities;

namespace ProductsService.Domain.Interfaces;

public interface IProductsRepository : IRepository<Product, Guid>
{
    Task<PagedList<Product>> GetProductsAsync(
        ProductFilter productFilter,
        SortParams sortParams,
        PageParams pageParams,
        CancellationToken cancellationToken = default);

    Task<Product?> GetAllInfoByIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default);
    
    Task<Product?> GetByIdWithImagesAsync(
        Guid productId,
        CancellationToken cancellationToken = default);
    
    Task<Product?> GetByIdWithCategoriesAsync(
        Guid productId,
        CancellationToken cancellationToken = default);

    Task<Product?> GetByIdWithCharacteristicsAsync(
        Guid productId,
        CancellationToken cancellationToken = default);

    Task<int?> GetQuantityById(
        Guid productId,
        CancellationToken cancellationToken = default);
    
    Task<List<Product>> GetProductsByIdsAsync(
        List<Guid> productIds,
        CancellationToken cancellationToken = default);

    Task<IDbContextTransaction> BeginTransactionAsync(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default);
}