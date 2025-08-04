using MediatR;

namespace OrdersService.Application.Features.Orders.Delete;

public record DeleteOrderCommand(Guid OrderId) : IRequest;