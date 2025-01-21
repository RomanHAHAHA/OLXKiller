using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OLXKiller.API.Extensions;
using OLXKiller.Application.Abstractions;

namespace OLXKiller.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUsersService _usersService;

    public UsersController(IUsersService usersService)
    {
        _usersService = usersService;
    }


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

    [HttpPost("create-avatar")]
    [Authorize]
    public async Task<IActionResult> CreateAvatar(IFormFile image)
    {
        using var stream = image.OpenReadStream();
        var response = await _usersService.CreateAvatar(stream, User.GetId());

        if (response.IsSuccess)
        {
            return Ok();
        }

        return Conflict(new { description = response.Description });
    }

    [HttpPost("update-avatar")]
    [Authorize]
    public async Task<IActionResult> UpdateAvatar(IFormFile image)
    {
        using var stream = image.OpenReadStream();
        var response = await _usersService.UpdateAvatar(stream, User.GetId());

        if (response.IsSuccess)
        {
            return Ok();
        }

        return NotFound(new { description = response.Description });
    }

}
