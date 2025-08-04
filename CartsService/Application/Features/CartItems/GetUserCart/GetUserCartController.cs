using Common.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CartsService.Application.Features.CartItems.GetUserCart;

[ApiController]
[Route("api/carts")]
public class GetUserCartController(IMediator mediator) : ControllerBase
{
    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> GetUserCartAsync(CancellationToken cancellationToken)
    {
        var query = new GetUserCartQuery(User.GetId());
        var cartItems = await mediator.Send(query, cancellationToken);
        return Ok(new { data = cartItems });
    }
}