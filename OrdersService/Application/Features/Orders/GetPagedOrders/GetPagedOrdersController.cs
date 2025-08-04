using Common.API.Authentication;
using Common.Domain.Dtos;
using Common.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace OrdersService.Application.Features.Orders.GetPagedOrders;

[ApiController]
[Route("api/orders")]
public class GetPagedOrdersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [HasPermission(PermissionEnum.ManageOrders)]
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