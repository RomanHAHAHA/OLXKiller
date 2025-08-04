using Common.Domain.Models.Results;
using MediatR;

namespace ProductsService.Application.Features.ProductCharacteristics.Remove;

public record RemoveCharacteristicCommand(Guid ProductId, string Name) : IRequest<ApiResponse>;