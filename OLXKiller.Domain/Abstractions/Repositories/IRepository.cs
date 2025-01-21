using OLXKiller.Domain.Enums;

namespace OLXKiller.Domain.Abstractions.Repositories;

public interface IRepository<T>
{
    Task CreateAsync(T entity);

    IQueryable<T> GetAllAsync();

    Task RemoveAsync(T entity);

    Task<T> UpdateAsync(T entity);

    Task<T?> GetByIdAsync(Guid id, TrackingEnable tracking = TrackingEnable.True);
}
