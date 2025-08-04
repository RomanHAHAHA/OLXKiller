using Common.Domain.Models.Results;
using MediatR;
using ProductsService.Domain.Dtos;

namespace ProductsService.Application.Features.ProductCharacteristics.Add;

public record AddCharacteristicsCommand(
    Guid ProductId,
    List<ProductCharacteristicViewDto> Characteristics) : IRequest<ApiResponse>;