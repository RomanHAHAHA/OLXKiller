using System.Security.Claims;
using Common.Application.Options;
using Common.Domain.Models;

namespace Common.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetId(this ClaimsPrincipal? user)
    {
        var userIdClaim = user?.FindFirst(CustomClaims.UserId);

        if (userIdClaim is null)
        {
            return Guid.Empty;
        }

        return Guid.TryParse(userIdClaim.Value, out var userId) ? 
            userId : 
            Guid.Empty;
    }
}