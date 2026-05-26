using MediatR;

namespace ProductsService.Application.Features.Products.Commands.RollBackUpdate;

public record RollbackProductUpdateCommand(Guid ProductId, Guid CorrelationId) : IRequest;