using Common.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ReviewsService.Application.Features.Reviews.GetProductReviews;

[Route("/api/reviews")]
[ApiController]
public class GetProductReviewsController(IMediator mediator) : ControllerBase
{
    [HttpGet("product/{productId:guid}")]
    [AllowAnonymous]
    public async Task<List<ProductReviewDto>> GetProductReviewsAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var query = new GetProductReviewsQuery(productId, User.GetId());
        return await mediator.Send(query, cancellationToken);
    }
}