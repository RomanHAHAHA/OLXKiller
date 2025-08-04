using MediatR;

namespace ReviewsService.Application.Features.Products.Update;

public record UpdateProductCommand(
    Guid CorrelationId,
    Guid ProductId,
    Guid UserId,
    string Name,
    decimal Price) : IRequest;