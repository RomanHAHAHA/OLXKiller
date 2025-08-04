using ReviewsService.Domain.Dtos;

namespace ReviewsService.Application.Features.Reviews.GetFilteredReviews;

public class ReviewToModerateDto
{
    public required Guid ProductId { get; init; }
    
    public required UserReviewDto User { get; init; }

    public required string Text { get; init; } 

    public required double Rate { get; init; } 
    
    public required string Status { get; init; }

    public required string CreatedAt { get; init; }
}