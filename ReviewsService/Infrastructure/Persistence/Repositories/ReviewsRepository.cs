using Microsoft.EntityFrameworkCore;
using ReviewsService.Application.Features.Reviews.GetFilteredReviews;
using ReviewsService.Application.Features.Reviews.GetProductReviews;
using ReviewsService.Domain.Dtos;
using ReviewsService.Domain.Entities;
using ReviewsService.Domain.Enums;
using ReviewsService.Domain.Interfaces;

namespace ReviewsService.Infrastructure.Persistence.Repositories;

public class ReviewsRepository(ReviewsDbContext dbContext) : IReviewsRepository
{
    public async Task CreateAsync(
        Review review,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Reviews.AddAsync(review, cancellationToken);
    }

    public async Task<Review?> GetByIdAsync(
        Guid userId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Reviews
            .FirstOrDefaultAsync(r => 
                r.ProductId == productId && r.UserId == userId, 
                cancellationToken);
    }
    
    public void Delete(Review review) => dbContext.Reviews.Remove(review);
    
    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await dbContext.SaveChangesAsync(cancellationToken) > 0;

    public async Task<double> GetAverageProductRatingAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var approvedReviews = dbContext.Reviews
            .AsNoTracking()
            .Where(r => r.ProductId == productId && r.Status == ReviewStatus.Approved);

        if (!await approvedReviews.AnyAsync(cancellationToken))
        {
            return 0;
        }

        return await approvedReviews
            .Select(r => r.Rate)
            .AverageAsync(cancellationToken);
    }
    
    public async Task<List<ProductReviewDto>> GetProductReviewsAsync(
        Guid productId,
        Guid currentUserId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Reviews
            .AsNoTracking()
            .AsSplitQuery()
            .Include(r => r.User)
            .Include(r => r.Votes)
            .Where(r => r.ProductId == productId && r.Status == ReviewStatus.Approved)
            .Select(r => new ProductReviewDto
            {
                UserId = r.UserId,
                ProductId = r.ProductId,
                NickName = r.User!.NickName,
                AvatarPath = r.User!.AvatarPath,
                Text = r.Text,
                Rate = r.Rate,
                CreatedAt = $"{r.CreatedAt.ToLocalTime():dd.MM.yyyy HH:mm}",
                LikesCount = r.Votes.Count(v => v.VoteType == VoteType.Like),
                DislikesCount = r.Votes.Count(v => v.VoteType == VoteType.Dislike),
                CurrentUserVote = r.Votes
                    .Where(v => v.UserId == currentUserId)
                    .Select(v => (int)v.VoteType) 
                    .FirstOrDefault() 
            })
            .ToListAsync(cancellationToken);
    }
    
    public async Task<List<ReviewToModerateDto>> GetPendingReviewsAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Reviews
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Product)
            .Where(r => r.Status == ReviewStatus.Pending)
            .OrderBy(r => r.CreatedAt)
            .Select(r => new ReviewToModerateDto
            {
                ProductId = r.ProductId,
                User = new UserReviewDto
                {
                    Id = r.UserId,
                    NickName = r.User!.NickName,
                    AvatarPath = r.User!.AvatarPath,
                },
                Status = r.Status.ToString(),
                Text = r.Text,
                Rate = r.Rate,
                CreatedAt = $"{r.CreatedAt.ToLocalTime():dd.MM.yyyy}",
            })
            .ToListAsync(cancellationToken);
    }
}