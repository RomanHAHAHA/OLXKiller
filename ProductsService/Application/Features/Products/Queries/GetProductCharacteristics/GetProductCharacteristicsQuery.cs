using MediatR;

namespace ProductsService.Application.Features.Products.Queries.GetProductCharacteristics;

public record GetProductCharacteristicsQuery(Guid ProductId) : IRequest<List<ProductCharacteristicDto>>;