namespace ProductsService.Application.Features.Categories.Queries.GetViewCategories;

public class CategoryNodeDto
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = string.Empty;

    public List<CategoryNodeDto> Children { get; set; } = [];

    public int ProductCount { get; set; }
}