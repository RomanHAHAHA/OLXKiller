using Common.Domain.Models.Results;
using MediatR;

namespace CartsService.Application.Features.CartItems.Delete;

public record DeleteItemCommand(Guid UserId, Guid ProductId) : IRequest<ApiResponse>;