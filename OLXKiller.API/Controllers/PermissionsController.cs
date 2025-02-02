using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OLXKiller.Application.Options;
using OLXKiller.Domain.Models;
using System.IdentityModel.Tokens.Jwt;

namespace OLXKiller.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PermissionsController(
    IOptions<CustomCookieOptions> options) : ControllerBase
{
    [HttpGet("my")]
    [Authorize]
    public IActionResult GetPermissionsFromToken()
    {
        var jwtToken = GetTokenFromCookies();

        if (jwtToken is null)
        {
            return NotFound(new { description = "token not found." });
        }

        var permissions = jwtToken.Claims
            .Where(c => c.Type == CustomClaims.Permissions)
            .Select(c => c.Value);

        return Ok(new { permissions });
    }

    [HttpGet("has-permission")]
    [Authorize]
    public IActionResult HasPermission(string permission)
    {
        var jwtToken = GetTokenFromCookies();

        if (jwtToken is null)
        {
            return NotFound(new { description = "token not found." });
        }

        var existingPermission = jwtToken.Claims
            .FirstOrDefault(c => c.Value == permission);

        return existingPermission is null ? NotFound() : Ok();
    }

    private JwtSecurityToken? GetTokenFromCookies()
    {
        if (!HttpContext.Request.Cookies.TryGetValue(options.Value.Name, out var token))
        {
            return null;
        }

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        return jwtToken;
    }
}
