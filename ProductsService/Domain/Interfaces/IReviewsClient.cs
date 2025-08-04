namespace ProductsService.Domain.Interfaces;

public interface IReviewsClient
{
    Task<double> GetProductRatingAsync(
        Guid productId,
        CancellationToken cancellationToken = default);
}