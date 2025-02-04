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
    [HttpPost("assign-to-user/{userId:guid}/{roleId:int}")]
    [HasPermission(Permission.AssignRoleToUser)]
    public async Task<IActionResult> AssignRoleToUser(Guid userId, int roleId)
    {
        var response = await _rolesService.AssignRoleToUserAsync(userId, roleId);

        if (response.IsSuccess)
        {
            return Ok();
        }

        return this.HandleErrorResponse(response);
    }
}
