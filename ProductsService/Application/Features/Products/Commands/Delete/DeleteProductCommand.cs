using Common.Domain.Models.Results;
using MediatR;

namespace ProductsService.Application.Features.Products.Commands.Delete;

public record DeleteProductCommand(Guid InitiatorUserId, Guid ProductId) : IRequest<ApiResponse>;