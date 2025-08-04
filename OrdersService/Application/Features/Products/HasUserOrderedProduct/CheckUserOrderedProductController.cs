using Common.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OrdersService.Application.Features.Products.HasUserOrderedProduct;

[Route("/api/orders")]
[ApiController]
public class CheckUserOrderedProductController(IMediator mediator) : ControllerBase
{
    [HttpGet("{productId:guid}")]
    [Authorize]
    public async Task<IActionResult> HasUserOrderedProductAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var command = new HasReceivedProductQuery(User.GetId(),productId);
        var response = await mediator.Send(command, cancellationToken);
        return this.HandleResponse(response);
    }
}