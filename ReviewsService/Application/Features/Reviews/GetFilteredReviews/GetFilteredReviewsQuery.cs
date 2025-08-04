using Common.Domain.Dtos;
using MediatR;

namespace ReviewsService.Application.Features.Reviews.GetFilteredReviews;

public record GetFilteredReviewsQuery(
    ReviewsFilter ReviewsFilter,
    SortParams SortParams) : IRequest<List<ReviewToModerateDto>>;