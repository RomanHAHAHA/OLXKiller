using Microsoft.EntityFrameworkCore.Storage;

namespace OLXKiller.Persistence.Abstractions;

public interface IUnitOfWork
{
    Task<IDbContextTransaction> BeginTransactionAsync();

    Task<int> SaveChangesAsync();
}