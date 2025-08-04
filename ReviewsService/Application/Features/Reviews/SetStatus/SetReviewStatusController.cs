using Common.API.Authentication;
using Common.API.Extensions;
using Common.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ReviewsService.Domain.Enums;

namespace ReviewsService.Application.Features.Reviews.SetStatus;

[Route("/api/reviews")]
[ApiController]
public class SetReviewStatusController(IMediator mediator) : ControllerBase
{
    [HttpPatch("{userId:guid}/{productId:guid}/status/{status}")]
    [HasPermission(PermissionEnum.ManageReviews)]
    public async Task<IActionResult> SetReviewStatusAsync(
        Guid userId,
        Guid productId,
        ReviewStatus status,
        CancellationToken cancellationToken)
    {
        var command = new SetReviewStatusCommand(userId, productId, status);
        var response = await mediator.Send(command, cancellationToken);
        return this.HandleResponse(response);
    }
}