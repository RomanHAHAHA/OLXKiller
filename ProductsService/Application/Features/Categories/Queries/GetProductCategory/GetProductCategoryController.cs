using Common.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ProductsService.Application.Features.Categories.Queries.GetProductCategory;

[Route("api/products")]
[ApiController]
public class GetProductCategoryController(IMediator mediator) : ControllerBase
{
    [HttpGet("{productId:guid}/category")]
    public async Task<IActionResult> GetProductCategoriesAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var query = new GetProductCategoryQuery(productId);
        var response = await mediator.Send(query, cancellationToken);
        return this.HandleResponse(response);
    }
}