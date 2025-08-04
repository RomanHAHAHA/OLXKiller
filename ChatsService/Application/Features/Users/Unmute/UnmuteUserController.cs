using Common.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatsService.Application.Features.Users.Unmute;

[ApiController]
[Route("api/users")]
public class UnmuteUserController(IMediator mediator) : ControllerBase
{
    [Authorize]
    [HttpPost("{userId:guid}/unmute")]
    public async Task<IActionResult> UnmuteUserAsync(Guid userId, CancellationToken cancellation)
    {
        var command = new UnmuteUserCommand(User.GetId(), userId);
        var response = await mediator.Send(command, cancellation);
        return this.HandleResponse(response);
    }
}