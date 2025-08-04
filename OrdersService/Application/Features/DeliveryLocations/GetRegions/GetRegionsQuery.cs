using MediatR;

namespace OrdersService.Application.Features.DeliveryLocations.GetRegions;

public record GetRegionsQuery : IRequest<List<object>>;