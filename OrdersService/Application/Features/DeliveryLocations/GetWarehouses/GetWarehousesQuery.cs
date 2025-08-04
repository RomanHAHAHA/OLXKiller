using MediatR;

namespace OrdersService.Application.Features.DeliveryLocations.GetWarehouses;

public record GetWarehousesQuery(string CityRef) : IRequest<List<object>>;