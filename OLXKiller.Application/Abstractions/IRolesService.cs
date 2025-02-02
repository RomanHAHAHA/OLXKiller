using OLXKiller.Domain.Abstractions.Models;

namespace OLXKiller.Application.Abstractions;

public interface IRolesService
{
    Task<IBaseResponse> AssignRoleToUserAsync(Guid userId, int roleId);
}