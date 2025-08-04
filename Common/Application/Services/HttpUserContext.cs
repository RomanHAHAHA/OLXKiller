using Common.Application.Options;
using Common.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Common.Application.Services;

public class HttpUserContext(
    IHttpContextAccessor httpContextAccessor,
    IOptions<CustomCookieOptions> options) : IHttpUserContext
{
    public Guid UserId => GetUserId();

    public string AccessToken => GetAccessToken();
    
    private Guid GetUserId()
    {
        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            return default;
        }

        var idClaim = httpContext.User.FindFirst(CustomClaims.UserId);

        return idClaim is null ? default : Guid.Parse(idClaim.Value);
    }
    
    private string GetAccessToken()
    {
        return httpContextAccessor.HttpContext!.Request.Cookies[options.Value.Name]!;
    }
}