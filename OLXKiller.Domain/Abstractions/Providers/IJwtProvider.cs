using OLXKiller.Domain.Entities;

namespace OLXKiller.Domain.Abstractions.Providers;

public interface IJwtProvider
{
    Task<string> GenerateTokenAsync(UserEntity user);
}