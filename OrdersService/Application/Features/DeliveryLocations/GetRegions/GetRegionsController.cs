using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OrdersService.Application.Features.DeliveryLocations.GetRegions;

[Route("/api/delivery-locations")]
[ApiController]
public class GetRegionsController(IMediator mediator) : ControllerBase
{
    [HttpGet("regions")]
    [AllowAnonymous]
    public async Task<List<object>> GetRegionsAsync(CancellationToken cancellationToken)
    {
        var query = new GetRegionsQuery();
        return await mediator.Send(query, cancellationToken);
    }
}