using Microsoft.EntityFrameworkCore;
using ReviewsService.Domain.Entities;
using ReviewsService.Domain.Interfaces;

namespace ReviewsService.Infrastructure.Persistence.Repositories;

public class UsersRepository(ReviewsDbContext dbContext) : IUsersRepository
{
    public async Task CreateAsync(
        UserSnapshot user,
        CancellationToken cancellationToken = default)
    {
        await dbContext.UserSnapshots.AddAsync(user, cancellationToken);
    }

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await dbContext.SaveChangesAsync(cancellationToken) > 0; 
    
    public async Task<UserSnapshot?> GetByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.UserSnapshots
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.UserSnapshots
            .AnyAsync(u => u.Id == userId, cancellationToken);
    }

    public void Delete(UserSnapshot user) => dbContext.UserSnapshots.Remove(user);
}