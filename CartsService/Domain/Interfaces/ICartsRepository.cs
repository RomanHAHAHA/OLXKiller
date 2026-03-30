using CartsService.Domain.Entities;

namespace CartsService.Domain.Interfaces;

public interface ICartsRepository
{
    void Delete(CartItem cartItem);

    Task<CartItem?> GetByIdAsync(
        Guid userId,
        Guid productId,
        CancellationToken cancellationToken = default);
    
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
    
    Task<List<CartItem>> GetUserCartByIdAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);
    
    Task<bool> IsProductAlreadyInCartAsync(
        Guid productId,
        Guid userId,
        CancellationToken cancellationToken = default);
}