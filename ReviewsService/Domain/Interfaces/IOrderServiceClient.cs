using Common.Domain.Models.Results;

namespace ReviewsService.Domain.Interfaces;

public interface IOrderServiceClient
{
    Task<ApiResponse<bool>> HasUserOrderedProductAsync(
        Guid productId,
        CancellationToken cancellationToken = default);
}