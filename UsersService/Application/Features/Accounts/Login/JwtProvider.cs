using System.Security.Claims;
using System.Text;
using Common.Application.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using UsersService.Domain.Entities;
using UsersService.Domain.Interfaces;

namespace UsersService.Application.Features.Accounts.Login;

public class JwtProvider(
    IOptions<JwtOptions> options,
    IUsersRepository usersRepository) : IJwtProvider
{
    private readonly JwtOptions _options = options.Value;
    
    public async Task<string> GenerateTokenAsync(
        User user,
        CancellationToken cancellationToken = default)
    {
        var claims = await SetClaimsAsync(user, cancellationToken);
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));

        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var handler = new JsonWebTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            SigningCredentials = signingCredentials,
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(_options.ExpiredHours)
        };

        return handler.CreateToken(tokenDescriptor);
    }

    private async Task<List<Claim>> SetClaimsAsync(
        User user, 
        CancellationToken cancellationToken = default)
    {
        var roleName = await usersRepository.GetRoleNameAsync(user, cancellationToken);
        var claims = new List<Claim>
        {
            new(CustomClaims.UserId, user.Id.ToString()),
            new(CustomClaims.NickName, user.NickName),
            new(CustomClaims.Role, roleName ?? "Unknown"),
            new(CustomClaims.AvatarImageName, user.AvatarPath ?? string.Empty),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString())
        };

        var permissions = await usersRepository.GetPermissionsAsync(user.Id, cancellationToken);
        var permissionClaims = permissions
            .Select(p => new Claim(CustomClaims.Permissions, p.ToString()));

        claims.AddRange(permissionClaims);

        return claims;
    }
}