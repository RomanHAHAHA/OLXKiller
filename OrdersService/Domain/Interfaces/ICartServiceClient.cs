using Common.Domain.Dtos;

namespace OrdersService.Domain.Interfaces;

public interface ICartServiceClient
{
    Task<IReadOnlyList<CartItemDto>> GetCartItemsAsync(
        Guid userId, 
        CancellationToken cancellationToken);
}