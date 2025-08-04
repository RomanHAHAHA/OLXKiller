using Common.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CartsService.Application.Features.CartItems.Increment;

[ApiController]
[Route("api/carts")]
public class IncrementProductController(IMediator mediator) : ControllerBase
{
    [Authorize]
    [HttpPatch("{productId:guid}/increment")]
    public async Task<IActionResult> IncrementProductCountAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var command = new IncrementItemQuantityCommand(User.GetId(), productId);
        var response = await mediator.Send(command, cancellationToken);
        return this.HandleResponse(response);
    }
}