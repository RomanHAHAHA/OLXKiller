using Common.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ProductsService.Application.Features.Products.Queries.GetProductBase;

[Route("api/products")]
[ApiController]
public class GetProductBaseController(IMediator mediator) : Controller
{
    [HttpGet("{productId:guid}/base")]
    public async Task<IActionResult> GetProductBaseAsync(
        Guid productId, 
        CancellationToken cancellationToken)
    {
        var query = new GetProductBaseQuery(productId);
        var response = await mediator.Send(query, cancellationToken);
        return this.HandleResponse(response);
    }
}