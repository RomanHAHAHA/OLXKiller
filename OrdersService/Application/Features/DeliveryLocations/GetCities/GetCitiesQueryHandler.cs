using MediatR;
using OrdersService.Domain.Interfaces;

namespace OrdersService.Application.Features.DeliveryLocations.GetCities;

public class GetCitiesQueryHandler(
    INovaPoshtaClient novaPoshtaClient) : IRequestHandler<GetCitiesQuery, List<object>>
{
    public async Task<List<object>> Handle(
        GetCitiesQuery request, 
        CancellationToken cancellationToken)
    {
        return await novaPoshtaClient.GetCitiesAsync(request.RegionRef, cancellationToken);  
    }
}