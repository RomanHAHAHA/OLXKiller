namespace ProductsService.Application.Features.Products.Queries.GetPagedList;

public class ProductFilter
{
    public string? Name { get; set; }
    
    public decimal? Price { get; set; }
    
    public bool? IsAvailable { get; set; }
    
    public double? Rating { get; set; }

    public ICollection<Guid> CategoriesIds { get; set; } = [];
    
    public Guid UserId { get; set; } = Guid.Empty;
    
    public ProductFilterMode FilterMode { get; set; } = ProductFilterMode.AllProducts;
}

public class FilterCategoryNode
{
    public Guid Id { get; set; }

    public bool Selected { get; set; }

    public int Level { get; set; }

    public List<FilterCategoryNode> SubCategories { get; set; } = [];
}