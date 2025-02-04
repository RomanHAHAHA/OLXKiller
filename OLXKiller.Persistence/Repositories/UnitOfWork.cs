using Microsoft.EntityFrameworkCore.Storage;
using OLXKiller.Persistence.Abstractions;
using OLXKiller.Persistence.Contexts;

namespace OLXKiller.Persistence.Repositories;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await context.Database.BeginTransactionAsync();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await context.SaveChangesAsync();
    }
}
