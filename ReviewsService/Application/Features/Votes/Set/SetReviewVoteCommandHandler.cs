using Common.Domain.Models.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReviewsService.Domain.Entities;
using ReviewsService.Infrastructure.Persistence;

namespace ReviewsService.Application.Features.Votes.Set;

public class SetReviewVoteCommandHandler(
    ReviewsDbContext dbContext) : IRequestHandler<SetReviewVoteCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(SetReviewVoteCommand request, CancellationToken cancellationToken)
    {
        var vote = await dbContext.ReviewsVotes
            .FirstOrDefaultAsync(v => 
                    v.UserId == request.CurrentUserId && 
                    v.ReviewUserId == request.ReviewUserId && 
                    v.ReviewProductId == request.ReviewProductId,
                cancellationToken);

        if (vote is null)
        {   
            await dbContext.ReviewsVotes.AddAsync(new ReviewVote
            {
                UserId = request.CurrentUserId,
                ReviewUserId = request.ReviewUserId,
                ReviewProductId = request.ReviewProductId,
                VoteType = request.VoteType
            }, 
            cancellationToken);
        }
        else if (vote.VoteType == request.VoteType)
        {
            dbContext.ReviewsVotes.Remove(vote);
        }
        else
        {
            vote.VoteType = request.VoteType;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        
        return ApiResponse.Ok();
    }
}