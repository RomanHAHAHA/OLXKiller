using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OLXKiller.API.Authentication;
using OLXKiller.API.Extensions;
using OLXKiller.Application.Abstractions;
using OLXKiller.Domain.Enums;

namespace OLXKiller.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController(
    IUsersService _usersService) : ControllerBase
{
    [HttpGet("get-view-data")]
    [Authorize]
    public async Task<IActionResult> GetLoginedUserViewModel()
    {
        var response = await _usersService.GetLoginedUserView(User.GetId());

        if (response.IsSuccess)
        {
            return Ok(new { data = response.Data });
        }

        return NotFound(new { description = response.Description });
    }

    [HttpPost("set-avatar")]
    [Authorize]
    public async Task<IActionResult> SetAvatar(IFormFile image)
    {
        using var stream = image.OpenReadStream();
        var response = await _usersService.SetAvatarAsync(stream, User.GetId());

        if (response.IsSuccess)
        {
            return Ok();
        }

        return Conflict(new { description = response.Description });
    }

    [HttpGet("grouped-by-role")]
    [HasPermission(Permission.AssignRoleToUser)]
    public async Task<IActionResult> GetAllAppUsers()
    {
        var users = await _usersService.GetGroupedUsers();

        return Ok(new { data = users });
    }
}
