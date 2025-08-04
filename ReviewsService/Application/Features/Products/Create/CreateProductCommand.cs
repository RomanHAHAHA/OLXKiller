using MediatR;

namespace ReviewsService.Application.Features.Products.Create;

public record CreateProductCommand(
    Guid CorrelationId,
    Guid ProductId,
    Guid SellerId,
    string Name,
    decimal Price) : IRequest;