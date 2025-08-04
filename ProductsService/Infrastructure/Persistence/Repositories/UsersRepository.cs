using Microsoft.EntityFrameworkCore;
using ProductsService.Domain.Entities;
using ProductsService.Domain.Interfaces;

namespace ProductsService.Infrastructure.Persistence.Repositories;

public class UsersRepository(ProductsDbContext dbContext) : IUsersRepository
{
    public async Task CreateAsync(UserSnapshot user, CancellationToken cancellationToken = default)
    {
        await dbContext.UserSnapshots.AddAsync(user, cancellationToken);
    }
    
    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await dbContext.SaveChangesAsync(cancellationToken) > 0;

    public async Task<UserSnapshot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.UserSnapshots.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }
}