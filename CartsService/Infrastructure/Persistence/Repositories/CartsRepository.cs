using CartsService.Domain.Entities;
using CartsService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CartsService.Infrastructure.Persistence.Repositories;

public class CartsRepository(CartsDbContext dbContext) : ICartsRepository
{
    public void Delete(CartItem cartItem) => dbContext.CartItems.Remove(cartItem);

    public async Task<CartItem?> GetByIdAsync(
        Guid userId, 
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.CartItems
            .FirstOrDefaultAsync(ci => 
                ci.UserId == userId && ci.ProductId == productId, 
                cancellationToken);
    }

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await dbContext.SaveChangesAsync(cancellationToken) > 0; 

    public async Task<List<CartItem>> GetUserCartByIdAsync(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        return await dbContext.CartItems
            .AsNoTracking()
            .Include(ci => ci.ProductSnapshot)
            .Where(ci => ci.UserId == userId)
            .ToListAsync(cancellationToken);
    }
}