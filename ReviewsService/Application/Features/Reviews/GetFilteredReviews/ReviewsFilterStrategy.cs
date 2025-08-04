using Common.Domain.Extensions;
using Common.Domain.Interfaces;
using ReviewsService.Domain.Entities;

namespace ReviewsService.Application.Features.Reviews.GetFilteredReviews;

public class ReviewsFilterStrategy : IFilterStrategy<Review, ReviewsFilter>
{
    public IQueryable<Review> Filter(IQueryable<Review> query, ReviewsFilter filter)
    {
        return query
            .WhereIf(filter.UserId.HasValue, r =>
                r.UserId == filter.UserId!.Value)
            
            .WhereIf(filter.ProductId.HasValue, r =>
                r.UserId == filter.ProductId!.Value)
            
            .WhereIf(filter.Status.HasValue, r =>
                r.Status == filter.Status!.Value)
            
            .WhereIf(filter.Rate.HasValue, r =>
                r.Rate >= filter.Rate)

            .WhereIf(filter.DateFrom.HasValue, r =>
                r.CreatedAt >= filter.DateFrom)

            .WhereIf(filter.DateTo.HasValue, r =>
                r.CreatedAt <= filter.DateTo);
    }
}