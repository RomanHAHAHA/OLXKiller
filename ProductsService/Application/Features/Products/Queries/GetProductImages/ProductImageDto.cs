namespace ProductsService.Application.Features.Products.Queries.GetProductImages;

public class ProductImageDto
{
    public required Guid Id { get; init; }

    public required string ImageName { get; init; }

    public required bool IsMain { get; init; }
}