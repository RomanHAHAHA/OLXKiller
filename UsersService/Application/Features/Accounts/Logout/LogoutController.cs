using Common.Application.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace UsersService.Application.Features.Accounts.Logout;

[Route("api/users")]
[ApiController]
public class LogoutController(IOptions<CustomCookieOptions> options) : Controller
{
    [Authorize]
    [HttpDelete("logout")]
    public IActionResult LogOut()
    {
        HttpContext.Response.Cookies.Delete(options.Value.Name);
        
        return Ok();
    }
}