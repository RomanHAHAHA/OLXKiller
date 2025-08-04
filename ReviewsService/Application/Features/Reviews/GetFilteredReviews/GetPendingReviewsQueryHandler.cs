using Common.Domain.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReviewsService.Domain.Dtos;
using ReviewsService.Infrastructure.Persistence;

namespace ReviewsService.Application.Features.Reviews.GetFilteredReviews;

public class GetPendingReviewsQueryHandler(
    ReviewsDbContext dbContext) : IRequestHandler<GetFilteredReviewsQuery, List<ReviewToModerateDto>>
{
    public async Task<List<ReviewToModerateDto>> Handle(
        GetFilteredReviewsQuery request, 
        CancellationToken cancellationToken)
    {
        var reviews = await dbContext.Reviews
            .AsNoTracking()
            .AsSplitQuery()
            .Include(r => r.Product)
            .Include(r => r.User)
            .Filter(request.ReviewsFilter)
            .Sort(request.SortParams)
            .ToListAsync(cancellationToken);

        return reviews.Select(r => new ReviewToModerateDto
        {
            ProductId = r.ProductId,
            User = new UserReviewDto
            {
                Id = r.UserId,
                NickName = r.User!.NickName,
                AvatarPath = r.User!.AvatarPath,
            },
            Text = r.Text,
            Rate = r.Rate,
            Status = r.Status.ToString(),
            CreatedAt = $"{r.CreatedAt.ToLocalTime():dd.MM.yyyy HH:mm}",
        }).ToList();
    }
}