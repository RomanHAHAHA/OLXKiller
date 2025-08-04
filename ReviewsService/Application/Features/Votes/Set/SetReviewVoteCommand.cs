using Common.Domain.Models.Results;
using MediatR;
using ReviewsService.Domain.Enums;

namespace ReviewsService.Application.Features.Votes.Set;

public record SetReviewVoteCommand(
    Guid CurrentUserId, 
    Guid ReviewUserId,
    Guid ReviewProductId,
    VoteType VoteType) : IRequest<ApiResponse>;