using Common.Domain.Abstractions;
using Common.Domain.Interfaces;
using UsersService.Application.Features.Accounts.Register;

namespace UsersService.Domain.Entities;

public class User : Entity<Guid>
{
    public string NickName { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    public string PasswordHash { get; set; } = string.Empty;
    
    public bool EmailConfirmed { get; set; }

    public string? AvatarPath { get; set; } = string.Empty;
    
    public int RoleId { get; set; }
    
    public Role? Role { get; set; }

    public DateTime? LastLogIn { get; set; }

    public static User FromRegisterDto(UserRegisterDto registerDto, IPasswordHasher passwordHasher)
    {
        return new User
        {
            NickName = registerDto.NickName,
            Email = registerDto.Email,
            PasswordHash = passwordHasher.HashPassword(registerDto.Password),
            RoleId = 1,
            EmailConfirmed = false,
        };
    }
}