namespace ReviewsService.Domain.Dtos;

public class UserReviewDto
{
    public required Guid Id { get; init; }
    
    public required string NickName { get; init; }

    public required string AvatarPath { get; init; }
}