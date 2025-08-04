using Common.Domain.Models.Results;
using MediatR;

namespace CartsService.Application.Features.CartItems.Create;

public record AddProductToCartCommand(Guid UserId, Guid ProductId) : IRequest<ApiResponse>;