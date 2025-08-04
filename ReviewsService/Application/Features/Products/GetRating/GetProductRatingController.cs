using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ReviewsService.Application.Features.Products.GetRating;

[Route("/api/reviews")]
[ApiController]
public class GetProductRatingController(IMediator mediator) : ControllerBase
{
    [HttpGet("product/{productId:guid}/rating")]
    public async Task<IActionResult> GetProductRatingAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var query = new GetProductRatingQuery(productId);
        var rating = await mediator.Send(query, cancellationToken);
        return Ok(new { data = rating });
    }
}