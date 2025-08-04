using Common.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatsService.Application.Features.Users.Mute;

[ApiController]
[Route("api/users")]
public class MuteUserController(IMediator mediator) : ControllerBase
{
    [Authorize]
    [HttpPost("{userId:guid}/mute")]
    public async Task<IActionResult> MuteUserAsync(Guid userId, CancellationToken cancellation)
    {
        var command = new MuteUserCommand(User.GetId(), userId);
        var response = await mediator.Send(command, cancellation);
        return this.HandleResponse(response);
    }
}