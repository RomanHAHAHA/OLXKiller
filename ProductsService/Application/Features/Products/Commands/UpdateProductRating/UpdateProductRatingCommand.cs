using MediatR;

namespace ProductsService.Application.Features.Products.Commands.UpdateProductRating;

public record UpdateProductRatingCommand(Guid ProductId, double AverageRating) : IRequest;