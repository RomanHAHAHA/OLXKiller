using System.Linq.Expressions;
using Common.Domain.Extensions;
using Common.Domain.Interfaces;
using ProductsService.Domain.Entities;
using ProductsService.Infrastructure.Persistence;

namespace ProductsService.Application.Features.Products.Queries.GetPagedList;

public class ProductFilterStrategy : IFilterStrategy<Product, ProductFilter>
{
    public IQueryable<Product> Filter(IQueryable<Product> query, ProductFilter filter)
    {
        var categoryIds = new HashSet<Guid>();
    
        /*if (filter.Categories.Count != 0)
        {
            var allCategories = dbContext.Categories.ToList();
            
            foreach (var categoryId in filter.Categories)
            {
                var category = allCategories.FirstOrDefault(c => c.Id == categoryId);
                
                if (category != null)
                {
                    var subcategoryIds = allCategories
                        .Where(c => c.Level >= category.Level)
                        .Select(c => c.Id)
                        .ToList();
                
                    categoryIds.UnionWith(subcategoryIds);
                }
            }
        }*/
    
        return query
            .WhereIf(!string.IsNullOrWhiteSpace(filter.Name), p => p.Name.StartsWith(filter.Name!))
            .WhereIf(filter.Price.HasValue, p => p.Price >= filter.Price)
            .WhereIf(filter.IsAvailable.HasValue, p => 
                filter.IsAvailable!.Value
                    ? p.StockQuantity > 0
                    : p.StockQuantity == 0)
            .WhereIf(filter.Rating.HasValue, p => p.AverageRating >= filter.Rating)
            //.WhereIf(categoryIds.Any(), p => categoryIds.Contains(p.CategoryId!.Value)) 
            .Where(GetFilterModePredicate(filter));
    }
    
    private Expression<Func<Product, bool>> GetFilterModePredicate(ProductFilter filter)
    {
        return filter.FilterMode switch
        {
            ProductFilterMode.MyProducts when filter.UserId != Guid.Empty =>
                p => p.UserId == filter.UserId,
                
            ProductFilterMode.ExcludeMyProducts when filter.UserId != Guid.Empty =>
                p => p.UserId != filter.UserId,
                
            _ => p => true 
        };
    }
}