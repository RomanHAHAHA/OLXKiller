using ReviewsService.Domain.Entities;

namespace ReviewsService.Domain.Interfaces;

public interface IUsersRepository
{
    Task CreateAsync(UserSnapshot user, CancellationToken cancellationToken = default);

    Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<UserSnapshot?> GetByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    void Delete(UserSnapshot user);
}