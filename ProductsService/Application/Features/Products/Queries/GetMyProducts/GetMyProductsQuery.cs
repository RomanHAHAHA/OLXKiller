using MediatR;

namespace ProductsService.Application.Features.Products.Queries.GetMyProducts;

public record GetMyProductsQuery(Guid UserId) : IRequest<List<MyProductDto>>;