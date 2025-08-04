namespace ProductsService.Application.Features.Products.Queries.GetProductCharacteristics;

public class ProductCharacteristicDto
{
    public required Guid Id { get; init; }
    
    public required string Name { get; init; }
    
    public required string Value { get; init; }
}