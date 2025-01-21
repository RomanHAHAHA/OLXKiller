using Microsoft.EntityFrameworkCore;
using OLXKiller.Domain.Entities;
using OLXKiller.Domain.Enums;
using OLXKiller.Domain.Models;
using System.Linq.Expressions;

namespace OLXKiller.Persistence.Extensions;

public static class QueriableExtensions
{
    public static IQueryable<T> WhereIf<T>(
       this IQueryable<T> queriable,
       bool condition,
       Expression<Func<T, bool>> predicate)
    {
        if (condition)
        {
            return queriable.Where(predicate);
        }

        return queriable;
    }

    public static IQueryable<ProductEntity> Filter(
        this IQueryable<ProductEntity> query,
        ProductFilter filter)
    {
        var result = query
            .WhereIf(!string.IsNullOrEmpty(filter.Name), 
                p => p.Name.StartsWith(filter.Name ?? string.Empty))
            .WhereIf(filter.MinPrice is not null, 
                p => p.Price >= filter.MinPrice)
            .WhereIf(filter.IsAvailable, 
                p => p.Amount > 0);

        return result;
    }

    public static IQueryable<ProductEntity> Sort(
        this IQueryable<ProductEntity> query,
        ProductSortParams sortParams)
    {
        var keySelector = GetKeySelector(sortParams.OrderBy);

        if (keySelector is null)
        {
            return query;
        }

        return sortParams.SortDirection == SortDirection.Descending
            ? query.OrderByDescending(keySelector)
            : query.OrderBy(keySelector);
    }

    private static Expression<Func<ProductEntity, object>>? GetKeySelector(string? orderBy)
    {
        if (string.IsNullOrEmpty(orderBy))
        {
            return null; 
        }

        return orderBy switch
        {
            nameof(ProductEntity.Name) => x => x.Name,
            nameof(ProductEntity.Price) => x => x.Price,
            _ => null 
        };
    }

    public static async Task<PagedResult<T>> ToPagedAsync<T>(
        this IQueryable<T> queriable,
        PageParams pageParams)
    {
        var count = await queriable.CountAsync();

        if (count == 0)
        {
            return new PagedResult<T>([], 0);
        }

        var page = pageParams.Page ?? 1;
        var pageSize = pageParams.PageSize ?? 12;

        var collection = await queriable
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<T>(collection, count);
    }
}
