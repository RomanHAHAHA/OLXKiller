using Common.API.Authentication;
using Common.Domain.Dtos;
using Common.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace ReviewsService.Application.Features.Reviews.GetFilteredReviews;

[ApiController]
[Route("api/reviews")]
public class GetFilteredReviewsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [HasPermission(PermissionEnum.ManageReviews)]
    public async Task<IActionResult> GetPendingReviewsAsync(
        [FromQuery] ReviewsFilter reviewsFilter,
        [FromQuery] SortParams sortParams,
        CancellationToken cancellationToken)
    {
        var query = new GetFilteredReviewsQuery(reviewsFilter, sortParams);
        var reviews = await mediator.Send(query, cancellationToken);
        return Ok(new { data = reviews });
    }
}