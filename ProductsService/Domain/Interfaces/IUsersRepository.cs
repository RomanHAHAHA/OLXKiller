using ProductsService.Domain.Entities;

namespace ProductsService.Domain.Interfaces;

public interface IUsersRepository
{
    Task CreateAsync(UserSnapshot user, CancellationToken cancellationToken = default);
    
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
    
    Task<UserSnapshot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}