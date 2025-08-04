using MediatR;

namespace OrdersService.Application.Features.Products.Delete;

public record DeleteProductCommand(Guid ProductId) : IRequest;