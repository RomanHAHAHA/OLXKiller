using OLXKiller.Domain.Entities;

namespace OLXKiller.Application.Dtos.User;

public class UserRegistrationDto
{
    public string NickName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string PasswordConfirm { get; set; } = string.Empty;

    public UserEntity AsUserEntity()
    {
        return new UserEntity
        {
            NickName = NickName,
            Email = Email,
            HashedPassword = Password,
        };
    }
}
