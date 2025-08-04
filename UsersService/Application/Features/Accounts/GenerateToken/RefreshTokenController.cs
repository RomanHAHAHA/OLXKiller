using Common.API.Extensions;
using Common.Application.Options;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace UsersService.Application.Features.Accounts.GenerateToken;

[Route("/api/accounts")]
[ApiController]
public class RefreshTokenController(
    IMediator mediator,
    IOptions<CustomCookieOptions> options) : ControllerBase
{
    [Authorize]
    [HttpGet("refresh-token")]
    public async Task<IActionResult> RefreshTokenAsync(CancellationToken cancellationToken)
    {
        var command = new GenerateTokenCommand(User.GetId());
        var response = await mediator.Send(command, cancellationToken);

        if (response.IsFailure)
        {
            return this.HandleResponse(response);
        }
        
        var token = response.Data;
        HttpContext.Response.Cookies.Append(options.Value.Name, token);

        return Ok(new { token });
    }
}