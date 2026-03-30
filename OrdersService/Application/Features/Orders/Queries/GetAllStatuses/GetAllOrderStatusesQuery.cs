using MediatR;

namespace OrdersService.Application.Features.Orders.Queries.GetAllStatuses;

public record GetAllOrderStatusesQuery : IRequest<List<DbOrderStatusDto>>;