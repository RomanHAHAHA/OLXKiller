using MediatR;
using OrdersService.Domain.Interfaces;

namespace OrdersService.Application.Features.DeliveryLocations.GetRegions;

public class GetRegionsQueryHandler(
    INovaPoshtaClient novaPoshtaClient) : IRequestHandler<GetRegionsQuery, List<object>>
{
    public async Task<List<object>> Handle(
        GetRegionsQuery request, 
        CancellationToken cancellationToken)
    {
        return await novaPoshtaClient.GetAreasAsync(cancellationToken);
    }
}