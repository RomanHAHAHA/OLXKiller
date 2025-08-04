using MediatR;

namespace ProductsService.Application.Features.Products.Queries.GetProductImages;

public record GetProductImagesQuery(Guid ProductId) : IRequest<List<ProductImageDto>>;