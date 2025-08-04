namespace ProductsService.Application.Features.Products.Queries.GetMyProducts;

public class MyProductDto
{
    public required Guid Id { get; init; }
    
    public required string Name { get; init; }
    
    public required decimal Price { get; init; }
    
    public required string MainImageName { get; init; }
    
    public required double Rating { get; init; }
}