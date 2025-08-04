using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OrdersService.Application.Features.DeliveryLocations.GetWarehouses;

[Route("/api/delivery-locations")]
[ApiController]
public class GetWarehousesController(IMediator mediator) : ControllerBase
{
    [HttpGet("warehouses/{cityRef}")]
    [AllowAnonymous]
    public async Task<List<object>> GetWarehousesAsync(
        string cityRef,
        CancellationToken cancellationToken)
    {
        var query = new GetWarehousesQuery(cityRef);
        return await mediator.Send(query, cancellationToken);
    }
}