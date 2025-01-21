using Microsoft.EntityFrameworkCore;
using OLXKiller.Domain.Abstractions.Repositories;
using OLXKiller.Domain.Enums;
using OLXKiller.Persistence.Contexts;

namespace OLXKiller.Persistence.Repositories;

public abstract class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _appDbContext;
    private readonly DbSet<T> _dbSet;

    protected Repository(AppDbContext context)
    {
        _appDbContext = context;
        _dbSet = _appDbContext.Set<T>();
    }

    public virtual async Task CreateAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _appDbContext.SaveChangesAsync();
    }

    public virtual IQueryable<T> GetAllAsync() => _dbSet.AsQueryable();

    public virtual async Task RemoveAsync(T entity)
    {
        _dbSet.Remove(entity);
        await _appDbContext.SaveChangesAsync();
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _appDbContext.SaveChangesAsync();

        return entity;
    }

    public virtual async Task<T?> GetByIdAsync(
        Guid id,
        TrackingEnable tracking = TrackingEnable.True)
    {
        var result = await _dbSet.FindAsync(id);

        if (tracking == TrackingEnable.False)
        {
            _appDbContext.ChangeTracker.Clear();
        }

        return result;
    }
}
