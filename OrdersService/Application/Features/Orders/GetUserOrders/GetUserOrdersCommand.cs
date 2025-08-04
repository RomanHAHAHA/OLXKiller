using MediatR;
using OrdersService.Domain.Dtos;

namespace OrdersService.Application.Features.Orders.GetUserOrders;

public record GetUserOrdersCommand(Guid UserId) : IRequest<List<PersonalOrderDto>>;