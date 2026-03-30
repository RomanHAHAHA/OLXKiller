using MediatR;

namespace OrdersService.Application.Features.Orders.Commands.Delete;

public record DeleteOrderCommand(Guid OrderId) : IRequest;