using MediatR;

namespace OrdersService.Application.Features.Orders.GetAllStatuses;

public record GetAllOrderStatusesQuery : IRequest<List<DbOrderStatusDto>>;