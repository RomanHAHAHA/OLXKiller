using ReviewsService.Domain.Enums;

namespace ReviewsService.Application.Features.Reviews.GetFilteredReviews;

public record ReviewsFilter(
    Guid? ProductId,
    Guid? UserId,
    double? Rate,
    ReviewStatus? Status,
    DateTime? DateFrom,
    DateTime? DateTo);