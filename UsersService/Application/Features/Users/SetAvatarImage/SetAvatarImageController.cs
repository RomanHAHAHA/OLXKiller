using Common.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UsersService.Application.Features.Users.SetAvatarImage;

[Route("/api/users")]
[ApiController]
public class SetAvatarImageController(IMediator mediator) : ControllerBase
{
    [Authorize]
    [HttpPatch("me/avatar")]
    public async Task<IActionResult> SetAvatarImageAsync(
        [FromForm] SetAvatarImageDto imageDto,
        CancellationToken cancellationToken)
    {
        var command = new SetAvatarImageCommand(User.GetId(), imageDto);
        var response = await mediator.Send(command, cancellationToken);
        return this.HandleResponse(response);
    }
}