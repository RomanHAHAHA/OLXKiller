using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ProductsService.Application.Features.Categories.Queries.GetProductCategories;

[Route("api/products")]
[ApiController]
public class GetProductCategoriesController(IMediator mediator) : ControllerBase
{
    [HttpGet("{productId:guid}/categories")]
    public async Task<IActionResult> GetProductCategoriesAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var query = new GetProductCategoriesQuery(productId);
        var categories = await mediator.Send(query, cancellationToken);
        return Ok(new { data = categories } );
    }
}