using System.Linq.Expressions;
using Common.Domain.Interfaces;
using ReviewsService.Domain.Entities;

namespace ReviewsService.Application.Features.Reviews.GetFilteredReviews;

public class ReviewSortStrategy : ISortStrategy<Review>
{
    public Expression<Func<Review, object>> GetKeySelector(string? orderBy)
    {
        return orderBy switch
        {
            nameof(Review.Rate) => p => p.Rate,
            nameof(Review.CreatedAt) => p => p.CreatedAt,
            nameof(Review.Status) => p => p.Status,
            _ => p => p.CreatedAt
        };
    }
}