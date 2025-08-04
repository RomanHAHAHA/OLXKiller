using ReviewsService.Domain.Enums;

namespace ReviewsService.Domain.Entities;

public class ReviewVote
{
    public Guid UserId { get; set; }

    public Guid ReviewUserId { get; set;}
    
    public Guid ReviewProductId { get; set; }
    
    public Review? Review { get; set; }
    
    public VoteType VoteType { get; set; }
}