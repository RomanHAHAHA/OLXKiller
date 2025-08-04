using Common.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReviewsService.Application.Features.Reviews.Create;

namespace ReviewsService.Application.Features.Reviews.Update;

[Route("/api/reviews")]
[ApiController]
public class UpdateReviewController(IMediator mediator) : ControllerBase
{
    [HttpPatch]
    [Authorize]
    public async Task<IActionResult> UpdateReviewAsync(
        ReviewCreateDto reviewCreateDto,
        CancellationToken cancellationToken)
    {
        var command = new UpdateReviewCommand(reviewCreateDto, User.GetId());
        var response = await mediator.Send(command, cancellationToken);
        return this.HandleResponse(response);
    }
}