using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OLXKiller.Application.Abstractions;
using OLXKiller.Application.Options;
using OLXKiller.Domain.Abstractions.Providers;
using OLXKiller.Domain.Entities;
using OLXKiller.Domain.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OLXKiller.Application.Providers;

public class JwtProvider(
    IOptions<JwtOptions> _options,
    IPermissionsService _permissionsService) : IJwtProvider
{
    private readonly JwtOptions _options = _options.Value;

    public async Task<string> GenerateTokenAsync(UserEntity user)
    {
        var claims = new List<Claim>
        {
            new(CustomClaims.UserId, user.Id.ToString()),
            new(CustomClaims.NickName, user.NickName),
        };

        var permissions = await _permissionsService.GetPermissionsAsync(user.Id);

        foreach (var permission in permissions)
        {
            claims.Add(new Claim(
                CustomClaims.Permissions, 
                permission.ToString()));
        }

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            signingCredentials: signingCredentials,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_options.ExpiredHours));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
