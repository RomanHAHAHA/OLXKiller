using MediatR;

namespace CartsService.Application.Features.CartItems.CleanCart;

public record CleanCartCommand(Guid UserId) : IRequest;