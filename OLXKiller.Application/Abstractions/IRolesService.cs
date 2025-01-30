using OLXKiller.Application.Dtos.Role;
using OLXKiller.Domain.Abstractions.Models;

namespace OLXKiller.Application.Abstractions;

public interface IRolesService
{
    Task<IBaseResponse> AssignRoleToUserAsync(string email, int roleId);

    Task<List<RoleForSelectDto>> GetRolesForSelectAsync();
}