namespace ProductsService.Application.Features.Products.Queries.GetPagedList;

public class ProductPagedDto
{
    public required Guid Id { get; init; }
    
    public required string Name { get; init; }

    public required decimal Price { get; init; }

    public required bool IsAvailable { get; init; }

    public required double Rating { get; init; }

    public required string MainImagePath { get; init; } = string.Empty;

    public required List<string> Categories { get; init; } = [];
}