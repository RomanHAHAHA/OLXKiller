using Common.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Common.API.Authentication;

public class PermissionRequirement(PermissionEnum permission) : IAuthorizationRequirement
{
    public PermissionEnum Permission { get; set; } = permission;
}