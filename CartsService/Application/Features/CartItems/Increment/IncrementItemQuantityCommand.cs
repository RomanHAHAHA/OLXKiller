using Common.Domain.Models.Results;
using MediatR;

namespace CartsService.Application.Features.CartItems.Increment;

public record IncrementItemQuantityCommand(
    Guid UserId,
    Guid ProductId) : IRequest<ApiResponse>;