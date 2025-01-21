using OLXKiller.Domain.Models;
using System.Security.Claims;

namespace OLXKiller.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetId(this ClaimsPrincipal user)
    {
        if (user is null)
        {
            return Guid.Empty;
        }

        var userIdClaim = user.FindFirst(CustomClaims.UserId);

        if (userIdClaim is null)
        {
            return Guid.Empty;
        }

        if (Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        return Guid.Empty;
    }

    public static bool IsAuthorized(this ClaimsPrincipal user)
    {
        if (user is null)
        {
            return false;
        }

        var userIdentity = user.Identity;

        if (userIdentity is null)
        {
            return false;
        }

        return userIdentity.IsAuthenticated;
    }
}
