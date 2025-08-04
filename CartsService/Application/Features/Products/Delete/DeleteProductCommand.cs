using MediatR;

namespace CartsService.Application.Features.Products.Delete;

public record DeleteProductCommand(Guid ProductId) : IRequest;