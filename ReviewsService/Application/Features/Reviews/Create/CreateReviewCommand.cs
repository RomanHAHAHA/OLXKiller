using Common.Domain.Models.Results;
using MediatR;

namespace ReviewsService.Application.Features.Reviews.Create;

public record CreateReviewCommand(
    ReviewCreateDto ReviewCreateDto,
    Guid UserId) : IRequest<ApiResponse>;