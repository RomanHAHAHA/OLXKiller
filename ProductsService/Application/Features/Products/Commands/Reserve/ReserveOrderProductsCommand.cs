using Common.Domain.Dtos;
using MediatR;

namespace ProductsService.Application.Features.Products.Commands.Reserve;

public record ReserveOrderProductsCommand(
    Guid OrderId, 
    Guid UserId,
    List<CartItemDto> CartItems) : IRequest;