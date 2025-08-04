using Common.API.Extensions;
using Common.Application.Options;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace UsersService.Application.Features.Accounts.Login;

[Route("/api/accounts")]
[ApiController]
public class LoginController(
    IMediator mediator,
    IOptions<CustomCookieOptions> options) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] UserLoginDto userLoginDto)
    {
        var response = await mediator.Send(new LoginUserCommand(userLoginDto));
        
        if (response.IsFailure)
        {
            return this.HandleResponse(response);
        }

        var token = response.Data;
        HttpContext.Response.Cookies.Append(options.Value.Name, token);

        return Ok(new { token });
    }
}