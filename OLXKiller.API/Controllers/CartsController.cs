using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OLXKiller.API.Extensions;
using OLXKiller.Application.Abstractions;

namespace OLXKiller.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CartsController(
    ICartsService _cartsService) : ControllerBase
{
    [HttpPost("add-product/{productId:guid}")]
    [Authorize]
    public async Task<IActionResult> AddProductToCart(Guid productId)
    {
        var response = await _cartsService.AddToCart(productId, User.GetId());

        if (response.IsFailure)
        {
            return this.HandleErrorResponse(response);
        }

        return Ok();
    }

    [HttpDelete("remove-product/{productId:guid}")]
    [Authorize]
    public async Task<IActionResult> RemoveProductFromCart(Guid productId)
    {
        var response = await _cartsService.RemoveFromCart(productId, User.GetId());

        if (response.IsFailure)
        {
            return this.HandleErrorResponse(response);
        }

        return Ok();
    }

    [HttpGet("my-cart-products")]
    [Authorize]
    public async Task<IActionResult> GetMyCartProducts()
    {
        var cartItems = await _cartsService.GetItems(User.GetId());

        return Ok(cartItems);
    }
}
