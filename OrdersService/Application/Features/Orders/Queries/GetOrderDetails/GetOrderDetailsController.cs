using Common.API.Authentication;
using Common.API.Extensions;
using Common.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace OrdersService.Application.Features.Orders.Queries.GetOrderDetails;

[Route("api/orders")]
[ApiController]
public class GetOrderDetailsController(IMediator mediator) : ControllerBase
{
    [HttpGet("{orderId:guid}/details")]
    [HasPermission(PermissionEnum.ManageOrders)]
    public async Task<IActionResult> GetOrderDetailsAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var query = new GetOrderDetailsQuery(orderId);
        var response = await mediator.Send(query, cancellationToken);
        return this.HandleResponse(response);
    }
}