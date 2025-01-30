using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using OLXKiller.Domain.Enums;

namespace OLXKiller.API.Authentication;

public class PermissionAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) : 
    DefaultAuthorizationPolicyProvider(options)
{
    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var policy = await base.GetPolicyAsync(policyName);

        if (policy is not null)
        {
            return policy;
        }

        var permission = Enum
            .GetValues<Permission>()
            .FirstOrDefault(p => p.ToString() == policyName);

        return new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement(permission))
            .Build();
    }
}
