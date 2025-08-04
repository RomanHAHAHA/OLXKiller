using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OrdersService.Application.Features.DeliveryLocations.GetCities;

[Route("/api/delivery-locations")]
[ApiController]
public class GetCitiesController(IMediator mediator) : ControllerBase
{
    [HttpGet("cities/{regionRef}")]
    [AllowAnonymous]
    public async Task<List<object>> GetCitiesAsync(
        string regionRef,
        CancellationToken cancellationToken)
    {
        var query = new GetCitiesQuery(regionRef);
        return await mediator.Send(query, cancellationToken);
    }
}