using OLXKiller.Domain.Abstractions.Providers;
using System.Security.Cryptography;
using System.Text;

namespace OLXKiller.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        var hashedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        var hash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();

        return hash;
    }

    public bool Verify(string password, string hashedPassword)
        => HashPassword(password).Equals(hashedPassword);
}
