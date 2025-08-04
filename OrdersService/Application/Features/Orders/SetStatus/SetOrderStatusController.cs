using Common.API.Authentication;
using Common.API.Extensions;
using Common.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace OrdersService.Application.Features.Orders.SetStatus;

[Route("/api/orders")]
[ApiController]
public class SetOrderStatusController(IMediator mediator) : ControllerBase
{
    [HttpPatch("{orderId:guid}/{status}")]
    [HasPermission(PermissionEnum.ManageOrders)]
    public async Task<IActionResult> SetOrderStatusAsync(
        Guid orderId,
        OrderStatus status,
        CancellationToken cancellationToken)
    {
        var command = new SetOrderStatusCommand(User.GetId(), orderId, status);
        var response = await mediator.Send(command, cancellationToken);
        return this.HandleResponse(response);
    }
}