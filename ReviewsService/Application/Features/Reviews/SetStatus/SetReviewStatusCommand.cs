using Common.Domain.Models.Results;
using MediatR;
using ReviewsService.Domain.Enums;

namespace ReviewsService.Application.Features.Reviews.SetStatus;

public record SetReviewStatusCommand(
    Guid UserId,
    Guid ProductId,
    ReviewStatus Status) : IRequest<ApiResponse>;