using Microsoft.EntityFrameworkCore;
using OLXKiller.Domain.Abstractions.Repositories;
using OLXKiller.Domain.Entities;
using OLXKiller.Persistence.Contexts;

namespace OLXKiller.Persistence.Repositories;

public class CartItemsRepository(AppDbContext appDbContext) :
    Repository<CartItemEntity>(appDbContext),
    ICartItemsRepository
{
    public async Task<List<CartItemEntity>> GetItemsByUserIdAsync(Guid userId)
    {
        return await _appDbContext.CartItems
            .AsNoTracking()
            .Include(ci => ci.Product)
            .Where(ci => ci.UserId == userId) 
            .Select(ci => new CartItemEntity
            {
                Quantity = ci.Quantity,
                ProductId = ci.ProductId,
                Product = ci.Product != null ? new ProductEntity
                {
                    Name = ci.Product.Name,
                    Price = ci.Product.Price,
                } : null 
            })
            .ToListAsync();
    }

    public async Task<CartItemEntity?> GetByIdAsync(Guid userId, Guid productId)
    {
        return await _appDbContext.CartItems
            .Include(ci => ci.Product)
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId);
    }
}
