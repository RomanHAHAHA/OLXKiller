using UsersService.Domain.Entities;

namespace UsersService.Domain.Interfaces;

public interface IJwtProvider
{
    Task<string> GenerateTokenAsync(User user, CancellationToken cancellationToken = default);
}