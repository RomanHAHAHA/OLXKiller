using Common.API.Extensions;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace ReviewsService.Application.Features.Reviews.HasReviewedProduct;

[ApiController]
[Route("api/reviews")]
public class HasReviewedProductController(IMediator mediator) : ControllerBase
{
    [HttpGet("has-reviewed-product/{productId:guid}")]
    [Authorize]
    public async Task<IActionResult> HasOrderedProductAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var query = new HasReviewedProductQuery(User.GetId(), productId);
        var response = await mediator.Send(query, cancellationToken);
        return this.HandleResponse(response);
    }
}