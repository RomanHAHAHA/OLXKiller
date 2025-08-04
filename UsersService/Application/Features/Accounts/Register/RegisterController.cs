using Common.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UsersService.Application.Features.Accounts.Register;

[Route("/api/accounts")]
[ApiController]
public class RegisterController(IMediator mediator) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] UserRegisterDto userRegisterDto)
    {
        var response = await mediator.Send(new RegisterUserCommand(userRegisterDto));
        return this.HandleResponse(response);
    }
}