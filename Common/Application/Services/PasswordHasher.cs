using System.Security.Cryptography;
using System.Text;
using Common.Domain.Interfaces;

namespace Common.Application.Services;

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        var hashedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        var hash = Convert.ToHexStringLower(hashedBytes);

        return hash;
    }

    public bool Verify(string password, string hashedPassword)
        => HashPassword(password).Equals(hashedPassword);
}