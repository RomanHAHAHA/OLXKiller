using Microsoft.AspNetCore.Mvc;
using OLXKiller.API.Authentication;
using OLXKiller.API.Extensions;
using OLXKiller.Application.Abstractions;
using OLXKiller.Application.Dtos.Role;
using OLXKiller.Domain.Enums;

namespace OLXKiller.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RolesController(IRolesService _rolesService) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<RoleForSelectDto>> GetRoles()
    {
        return await _rolesService.GetRolesForSelectAsync();
    }

    [HttpPost("assign-to-user")]
    [HasPermission(Permission.AssignRoleToUser)]
    public async Task<IActionResult> AssignRoleToUser(string userEmail, int roleId)
    {
        var response = await _rolesService.AssignRoleToUserAsync(userEmail, roleId);

        if (response.IsSuccess)
        {
            return Ok();
        }

        return this.CreateResponse(response.Status, response.Description);
    }
}
