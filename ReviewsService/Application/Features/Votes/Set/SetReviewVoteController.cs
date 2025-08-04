using Common.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReviewsService.Domain.Enums;

namespace ReviewsService.Application.Features.Votes.Set;

[Route("/api/review-votes")]
[ApiController]
public class SetReviewVoteController(IMediator mediator) : ControllerBase
{
    [Authorize]
    [HttpPost("{userId:guid}/{productId:guid}/{voteType}")]
    public async Task<IActionResult> SetReviewVoteAsync(
        Guid userId,
        Guid productId,
        VoteType voteType,
        CancellationToken cancellationToken)
    {
        var command = new SetReviewVoteCommand(User.GetId(), userId, productId, voteType);
        var response = await mediator.Send(command, cancellationToken);
        return this.HandleResponse(response);
    }
}