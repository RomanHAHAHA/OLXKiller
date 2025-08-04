using System.Linq.Expressions;
using Common.Application.Factories;
using Common.Domain.Dtos;
using Common.Domain.Enums;
using Common.Domain.Models.Results;
using Microsoft.EntityFrameworkCore;

namespace Common.Domain.Extensions;

public static class QueryExtensions
{
    public static IQueryable<T> WhereIf<T>(
        this IQueryable<T> query,
        bool condition,
        Expression<Func<T, bool>> predicate)
    {
        return condition ? query.Where(predicate) : query;
    }

    public static IQueryable<T> Filter<T, TFilter>(
        this IQueryable<T> query, 
        TFilter filter)
    {
        var strategy = FilterStrategyFactory.GetFilterStrategy<T, TFilter>();
        return strategy.Filter(query, filter);
    }
    
    public static IQueryable<T> Sort<T>(this IQueryable<T> query, SortParams sortParams)
    {
        var strategy = SortStrategyFactory.GetSortStrategy<T>();
        var keySelector = strategy.GetKeySelector(sortParams.OrderBy);

        return sortParams.SortDirection == SortDirection.Descending
            ? query.OrderByDescending(keySelector)
            : query.OrderBy(keySelector);
    }

    public static async Task<PagedList<T>> ToPagedAsync<T>(
        this IQueryable<T> query, 
        PageParams pageParams,
        CancellationToken cancellationToken = default)
    {
        var page = pageParams.Page ?? 1;
        var pageSize = pageParams.PageSize ?? 12;
        var count = await query.CountAsync(cancellationToken);

        if (count == 0)
        {
            return new PagedList<T>([], page, pageSize, 0);
        }

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<T>(items, page, pageSize, count);
    }
}