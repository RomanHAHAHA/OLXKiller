using MediatR;

namespace ProductsService.Application.Features.ProductCharacteristics.GetProductCharacteristics;

public record GetProductCharacteristicsQuery(Guid ProductId) : IRequest<List<ProductCharacteristicDto>>;