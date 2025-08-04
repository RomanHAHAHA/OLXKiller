using MediatR;

namespace ReviewsService.Application.Features.Products.GetRating;

public record GetProductRatingQuery(Guid ProductId) : IRequest<double>;