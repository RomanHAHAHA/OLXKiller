using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ProductsService.Application.Features.Products.Queries.GetProductCharacteristics;

[Route("api/products")]
[ApiController]
public class GetProductCharacteristicsController(IMediator mediator) : ControllerBase
{
    [HttpGet("{productId:guid}/characteristics")]
    public async Task<IActionResult> GetProductCharacteristicsAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var query = new GetProductCharacteristicsQuery(productId);
        var characteristics = await mediator.Send(query, cancellationToken);
        return Ok(new { data = characteristics });
    }
}