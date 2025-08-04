using Common.API.Extensions;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace ProductsService.Application.Features.Products.Queries.GetMyProducts;

[ApiController]
[Route("api/products")]
public class GetMyProductsController(IMediator mediator) : ControllerBase
{
    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyProductsAsync(CancellationToken cancellationToken)
    {
        var query = new GetMyProductsQuery(User.GetId());
        var products = await mediator.Send(query, cancellationToken);
        return Ok(new { data = products });
    }
}