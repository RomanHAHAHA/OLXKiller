using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CartsService.Application.Features.CartItems.GetUserCart;

[ApiController]
[Route("api/carts")]
public class GetUserCartByIdController(IMediator mediator) : ControllerBase
{
    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetUserCartAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var query = new GetUserCartQuery(userId);
        var cartItems = await mediator.Send(query, cancellationToken);
        return Ok(new { data = cartItems });
    }
}