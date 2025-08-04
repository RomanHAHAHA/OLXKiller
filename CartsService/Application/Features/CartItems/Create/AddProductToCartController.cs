using System.Net;
using CartsService.Application.Features.CartItems.Increment;
using Common.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CartsService.Application.Features.CartItems.Create;

[Route("api/carts")]
[ApiController]
public class AddProductToCartController(IMediator mediator) : ControllerBase
{
    [Authorize]
    [HttpPost("{productId:guid}")]
    public async Task<IActionResult> AddProductToCartAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetId();
        
        var addProductToCartCommand = new AddProductToCartCommand(userId, productId);
        var response = await mediator.Send(addProductToCartCommand, cancellationToken);

        if (response.Status == HttpStatusCode.Conflict)
        {
            var incrementProductCommand = new IncrementItemQuantityCommand(userId, productId);
            response = await mediator.Send(incrementProductCommand, cancellationToken);
        }
        
        return this.HandleResponse(response);
    }
}