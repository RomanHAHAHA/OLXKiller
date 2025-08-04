using Common.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OrdersService.Application.Features.Orders.GetUserOrders;

[Route("/api/orders")]
[ApiController]
public class GetUserOrdersController(IMediator mediator) : ControllerBase
{
    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetUserOrdersAsync(CancellationToken cancellationToken)
    {
        var command = new GetUserOrdersCommand(User.GetId());
        var orders = await mediator.Send(command, cancellationToken);
        return Ok(new { data = orders });
    }
}