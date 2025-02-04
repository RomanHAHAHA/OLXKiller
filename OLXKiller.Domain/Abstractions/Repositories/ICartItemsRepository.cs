using OLXKiller.Domain.Entities;

namespace OLXKiller.Domain.Abstractions.Repositories;

public interface ICartItemsRepository : IRepository<CartItemEntity>
{
    Task<List<CartItemEntity>> GetItemsByUserIdAsync(Guid userId);

    Task<CartItemEntity?> GetByIdAsync(Guid userId, Guid productId);
}
