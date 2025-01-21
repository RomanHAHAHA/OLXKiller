using OLXKiller.Domain.Entities;

namespace OLXKiller.Domain.Abstractions.Providers;

public interface IJwtProvider
{
    string GenerateToken(UserEntity user);
}