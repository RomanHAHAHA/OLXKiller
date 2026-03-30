namespace ProductsService.Application.Features.ProductCharacteristics.GetProductCharacteristics;

public class ProductCharacteristicDto
{
    public required Guid Id { get; init; }
    
    public required string Name { get; init; }
    
    public required string Value { get; init; }
}