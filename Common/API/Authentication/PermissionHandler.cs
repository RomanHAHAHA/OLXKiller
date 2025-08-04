using Common.Application.Options;
using Common.Domain.Models;
using Microsoft.AspNetCore.Authorization;

namespace Common.API.Authentication;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var permissions = context.User.Claims
            .Where(c => c.Type == CustomClaims.Permissions)
            .Select(c => c.Value)
            .ToHashSet();

        if (permissions.Contains(requirement.Permission.ToString()))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}