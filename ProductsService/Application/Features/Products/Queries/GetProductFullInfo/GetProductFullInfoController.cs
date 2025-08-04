using Common.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProductsService.Application.Features.Products.Queries.GetProductFullInfo;

[ApiController]
[Route("api/products")]
public class GetProductFullInfoController(IMediator mediator) : ControllerBase
{
    [HttpGet("{productId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductInfoAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var query = new GetProductInfoQuery(productId, User.GetId());
        var response = await mediator.Send(query, cancellationToken);
        return this.HandleResponse(response);
    }
}