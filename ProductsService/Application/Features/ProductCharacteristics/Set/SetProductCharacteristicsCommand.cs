using Common.Domain.Models.Results;
using MediatR;
using ProductsService.Domain.Dtos;

namespace ProductsService.Application.Features.ProductCharacteristics.Set;

public record SetProductCharacteristicsCommand(
    Guid ProductId,
    List<ProductCharacteristicViewDto> Characteristics) : IRequest<ApiResponse>;