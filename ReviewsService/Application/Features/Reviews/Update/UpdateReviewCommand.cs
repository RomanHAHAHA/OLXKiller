using Common.Domain.Models.Results;
using MediatR;
using ReviewsService.Application.Features.Reviews.Create;

namespace ReviewsService.Application.Features.Reviews.Update;

public record UpdateReviewCommand(
    ReviewCreateDto ReviewCreateDto,
    Guid UserId) : IRequest<ApiResponse>;