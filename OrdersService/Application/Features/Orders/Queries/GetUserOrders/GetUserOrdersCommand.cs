using MediatR;
using OrdersService.Domain.Dtos;

namespace OrdersService.Application.Features.Orders.Queries.GetUserOrders;

public record GetUserOrdersCommand(Guid UserId) : IRequest<List<PersonalOrderDto>>;