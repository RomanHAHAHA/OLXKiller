using Microsoft.AspNetCore.Authorization;
using OLXKiller.Domain.Enums;

namespace OLXKiller.API.Authentication;

public class PermissionRequirement(Permission permission) : IAuthorizationRequirement
{
    public Permission Permission { get; set; } = permission;
}
