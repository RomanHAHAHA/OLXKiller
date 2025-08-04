using MediatR;

namespace ReviewsService.Application.Features.Products.SetMainImage;

public record SetMainProductImageCommand(
    Guid CorrelationId,
    Guid ProductId, 
    string ImagePath) : IRequest;