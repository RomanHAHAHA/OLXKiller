using Common.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CartsService.Application.Features.CartItems.Decrement;

[ApiController]
[Route("api/carts")]
public class DecrementProductController(IMediator mediator) : ControllerBase
{
    [Authorize]
    [HttpPatch("{productId:guid}/decrement")]
    public async Task<IActionResult> DecrementProductCountAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var command = new DecrementItemQuantityCommand(User.GetId(), productId);
        var response = await mediator.Send(command, cancellationToken);
        return this.HandleResponse(response);
    }
}