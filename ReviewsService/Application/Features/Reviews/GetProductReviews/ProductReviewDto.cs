namespace ReviewsService.Application.Features.Reviews.GetProductReviews;

public class ProductReviewDto
{
    public required Guid ProductId { get; init; }
    
    public required Guid UserId { get; init; }
    
    public required string NickName { get; init; } 

    public required string AvatarPath { get; init; }

    public required string Text { get; init; } 

    public required double Rate { get; init; } 

    public required string CreatedAt { get; init; }
    
    public required int LikesCount { get; init; }
    
    public required int DislikesCount { get; init; }
    
    public required int CurrentUserVote { get; init; }
}