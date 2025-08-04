using MediatR;
using ReviewsService.Domain.Interfaces;

namespace ReviewsService.Application.Features.Reviews.GetProductReviews;

public class GetProductReviewsQueryHandler(
    IReviewsRepository reviewsRepository) : IRequestHandler<GetProductReviewsQuery, List<ProductReviewDto>>
{
    public async Task<List<ProductReviewDto>> Handle(
        GetProductReviewsQuery request, 
        CancellationToken cancellationToken)
    {
        return await reviewsRepository
            .GetProductReviewsAsync(
                request.ProductId,
                request.CurrentUserId, 
                cancellationToken);
    }
}