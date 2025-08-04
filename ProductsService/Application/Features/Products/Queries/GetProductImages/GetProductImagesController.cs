using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ProductsService.Application.Features.Products.Queries.GetProductImages;

[Route("api/products")]
[ApiController]
public class GetProductImagesController(IMediator mediator) : ControllerBase
{
    [HttpGet("{productId:guid}/images")]
    public async Task<IActionResult> GetProductImagesAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var query = new GetProductImagesQuery(productId);
        var images = await mediator.Send(query, cancellationToken);
        return Ok(new { data = images });
    }
}