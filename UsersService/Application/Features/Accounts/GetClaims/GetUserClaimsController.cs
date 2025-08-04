using Common.Application.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UsersService.Domain.Dtos;

namespace UsersService.Application.Features.Accounts.GetClaims;

[Route("/api/users")]
[ApiController]
public class GetUserClaimsController : Controller
{
    [Authorize]
    [HttpGet("get-claims")]
    public IActionResult GetUserClaimsData()
    {
        var userCookiesData = new UserCookieDataDto
        {
            UserId = User.FindFirst(CustomClaims.UserId)!.Value,
            NickName = User.FindFirst(CustomClaims.NickName)!.Value,
            AvatarImageName = User.FindFirst(CustomClaims.AvatarImageName)!.Value,
            Role = User.FindFirst(CustomClaims.Role)!.Value,
            Permissions = User.FindAll(CustomClaims.Permissions).Select(c => c.Value).ToList()
        };

        return Ok(new { userCookiesData });
    }
}