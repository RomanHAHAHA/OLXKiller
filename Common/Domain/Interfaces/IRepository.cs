using Common.Domain.Abstractions;

namespace Common.Domain.Interfaces;

public interface IRepository<TEntity, in TKey> where TEntity : Entity<TKey>
{
    Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
    
    Task<bool> ExistsAsync(TKey id, CancellationToken cancellationToken = default);
    
    void Delete(TEntity entity);
    
    void Update(TEntity entity);
    
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
}