using MediatR;

namespace OrdersService.Application.Features.Products.Update;

public record UpdateProductCommand(
    Guid CorrelationId,
    Guid ProductId,
    Guid UserId,
    string Name,
    decimal Price) : IRequest;