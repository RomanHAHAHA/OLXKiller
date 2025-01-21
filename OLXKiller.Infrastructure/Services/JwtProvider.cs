using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OLXKiller.Application.Options;
using OLXKiller.Domain.Abstractions.Providers;
using OLXKiller.Domain.Entities;
using OLXKiller.Domain.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OLXKiller.Infrastructure.Services;

public class JwtProvider : IJwtProvider
{
    private readonly JwtOptions _options;

    public JwtProvider(
        IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public string GenerateToken(UserEntity user)
    {
        var claims = new List<Claim>
        {
            new(CustomClaims.UserId, user.Id.ToString()),
            new(CustomClaims.NickName, user.NickName),
        };

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
