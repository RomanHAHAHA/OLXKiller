using Common.Domain.Models.Results;
using MediatR;

namespace ReviewsService.Application.Features.Reviews.HasReviewedProduct;

public record HasReviewedProductQuery(Guid UserId, Guid ProductId) : IRequest<ApiResponse<bool>>;