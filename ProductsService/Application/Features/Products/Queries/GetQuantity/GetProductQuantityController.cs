using Common.API.Extensions;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace ProductsService.Application.Features.Products.Queries.GetQuantity;

[ApiController]
[Route("api/products")]
public class GetProductQuantityController(IMediator mediator) : ControllerBase
{
    [HttpGet("{productId:guid}/quantity")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductQuantityAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var command = new GetQuantityQuery(productId);
        var response = await mediator.Send(command, cancellationToken);
        return this.HandleResponse(response);
    }
}