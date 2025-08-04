using MediatR;
using ReviewsService.Domain.Interfaces;

namespace ReviewsService.Application.Features.Products.GetRating;

public class GetProductRatingQueryHandler(
    IReviewsRepository reviewsRepository) : IRequestHandler<GetProductRatingQuery, double>
{
    public async Task<double> Handle(GetProductRatingQuery request, CancellationToken cancellationToken)
    {
        return await reviewsRepository
            .GetAverageProductRatingAsync(request.ProductId, cancellationToken);
    }
}