using MediatR;

namespace ProductsService.Application.Features.Products.Commands.CheckStockQuantity;

public record CheckProductStockQuantityCommand(
    Guid UserId,
    Guid ProductId, 
    int RequestedQuantity) : IRequest;