using System.Linq.Expressions;
using Common.Domain.Interfaces;
using ProductsService.Domain.Entities;

namespace ProductsService.Application.Features.Products.Queries.GetPagedList;

public class ProductSortStrategy : ISortStrategy<Product>
{
    public Expression<Func<Product, object>> GetKeySelector(string? orderBy)
    {
        return orderBy switch
        {
            nameof(Product.Name) => p => p.Name,
            nameof(Product.Price) => p => p.Price,
            nameof(Product.AverageRating) => p => p.AverageRating,
            _ => p => p.Id
        };
    }
}