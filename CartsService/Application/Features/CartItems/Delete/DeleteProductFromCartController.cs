using Common.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CartsService.Application.Features.CartItems.Delete;

[ApiController]
[Route("api/carts")]
public class DeleteProductFromCartController(IMediator mediator) : ControllerBase
{
    [Authorize]
    [HttpDelete("{productId:guid}")]
    public async Task<IActionResult> DeleteProductFromCartAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteItemCommand(User.GetId(), productId);
        var response = await mediator.Send(command, cancellationToken);
        return this.HandleResponse(response);
    }
}