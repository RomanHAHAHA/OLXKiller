using MediatR;

namespace ReviewsService.Application.Features.Products.Delete;

public record DeleteProductCommand(Guid ProductId) : IRequest;