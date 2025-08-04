using Common.Domain.Models.Results;
using MediatR;

namespace CartsService.Application.Features.CartItems.Decrement;

public record DecrementItemQuantityCommand(
    Guid UserId,
    Guid ProductId) : IRequest<ApiResponse>;