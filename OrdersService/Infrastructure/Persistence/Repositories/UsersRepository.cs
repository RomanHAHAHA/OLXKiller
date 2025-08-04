using Microsoft.EntityFrameworkCore;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Interfaces;

namespace OrdersService.Infrastructure.Persistence.Repositories;

public class UsersRepository(OrdersDbContext dbContext) : IUsersRepository
{
    public async Task CreateAsync(
        UserSnapshot user,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Users.AddAsync(user, cancellationToken);
    }

    public async Task<UserSnapshot?> GetByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public void Delete(UserSnapshot user)
    {
        dbContext.Users.Remove(user);
    }

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await dbContext.SaveChangesAsync(cancellationToken) > 0;
}