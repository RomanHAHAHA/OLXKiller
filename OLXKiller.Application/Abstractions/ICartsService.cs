using OLXKiller.Domain.Abstractions.Models;
using OLXKiller.Domain.Entities;

namespace OLXKiller.Application.Abstractions;

public interface ICartsService
{
    Task<IBaseResponse> AddToCart(Guid productId, Guid userId);

    Task<IEnumerable<CartItemEntity>> GetItems(Guid userId);

    Task<IBaseResponse> RemoveFromCart(Guid productId, Guid userId);
}