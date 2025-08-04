using MediatR;

namespace OrdersService.Application.Features.DeliveryLocations.GetCities;

public record GetCitiesQuery(string RegionRef) : IRequest<List<object>>;