using Common.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UsersService.Application.Features.Users.UpdatePassword;

[Route("/api/users")]
[ApiController]
public class UpdatePasswordController(IMediator mediator) : ControllerBase
{
    [Authorize]
    [HttpPatch("me/password")]
    public async Task<IActionResult> UpdatePasswordAsync( 
        [FromBody] UpdatePasswordDto updatePasswordDto,
        CancellationToken cancellationToken)
    {
        var command = new UpdatePasswordCommand(User.GetId(), updatePasswordDto);
        var response = await mediator.Send(command, cancellationToken);
        return this.HandleResponse(response);
    }
}