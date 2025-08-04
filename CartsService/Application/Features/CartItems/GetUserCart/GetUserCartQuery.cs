using MediatR;

namespace CartsService.Application.Features.CartItems.GetUserCart;

public record GetUserCartQuery(Guid UserId) : IRequest<CartDto>;