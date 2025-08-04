using Common.Domain.Models.Results;
using MediatR;

namespace ProductsService.Application.Features.ProductCharacteristics.Update;

public record UpdateCharacteristicCommand(
    Guid ProductId,
    ProductCharacteristicUpdateDto CharacteristicDto) : IRequest<ApiResponse>;