using ReviewsService.Application.Features.Reviews.Create;
using ReviewsService.Domain.Enums;

namespace ReviewsService.Domain.Entities;

public class Review 
{
    public Guid UserId { get; set; }

    public UserSnapshot? User { get; set; }

    public Guid ProductId { get; set; }

    public ProductSnapshot? Product { get; set; }

    public string Text { get; set; } = string.Empty;

    public int Rate { get; set; }
    
    public ReviewStatus Status { get; set; }
    
    public DateTime CreatedAt { get; set; }

    public List<ReviewVote> Votes { get; set; } = [];

    public static Review FromCreateDto(ReviewCreateDto reviewCreateDto, Guid userId)
    {
        return new Review
        {
            UserId = userId,
            ProductId = reviewCreateDto.ProductId,
            Text = reviewCreateDto.Text,
            Rate = reviewCreateDto.Rate,
            Status = ReviewStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        };
    }
}