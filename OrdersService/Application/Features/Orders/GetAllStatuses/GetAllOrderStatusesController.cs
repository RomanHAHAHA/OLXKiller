using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace OrdersService.Application.Features.Orders.GetAllStatuses;

[ApiController]
[Route("api/order-statuses")]
public class GetAllOrderStatusesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllOrderStatuses(CancellationToken cancellationToken)
    {
        var query = new GetAllOrderStatusesQuery();
        var statuses = await mediator.Send(query, cancellationToken);
        return Ok(new { data = statuses });
    }
}