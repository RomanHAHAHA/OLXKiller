using Common.Domain.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace OrdersService.Application.Features.Orders.Queries.GetPagedOrders;

[Route("/api/orders")]
[ApiController]
public class GetPagedOrdersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    //[HasPermission(PermissionEnum.ManageOrders)]
    public async Task<IActionResult> GetPagedOrdersAsync(
        [FromQuery] OrderFilter orderFilter,
        [FromQuery] SortParams sortParams,
        [FromQuery] PageParams pageParams,
        CancellationToken cancellationToken)
    {
        var query = new GetPagedOrdersQuery(orderFilter, sortParams, pageParams);
        var pagedOrders = await mediator.Send(query, cancellationToken);
        return Ok(new { data = pagedOrders });
    }
}