using MediatR;
using OrdersService.Domain.Interfaces;

namespace OrdersService.Application.Features.DeliveryLocations.GetWarehouses;

public class GetWarehousesQueryHandler(
    INovaPoshtaClient novaPoshtaClient) : IRequestHandler<GetWarehousesQuery, List<object>>
{
    public async Task<List<object>> Handle(
        GetWarehousesQuery request, 
        CancellationToken cancellationToken)
    {
        return await novaPoshtaClient.GetWarehousesAsync(request.CityRef, cancellationToken);
    }
}