namespace OLXKiller.Domain.Abstractions.Providers;

public interface IPasswordHasher
{
    string HashPassword(string password);

    bool Verify(string password, string hashedPassword);
}
