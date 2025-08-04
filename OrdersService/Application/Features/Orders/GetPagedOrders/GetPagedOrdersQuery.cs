using Common.Domain.Dtos;
using Common.Domain.Models.Results;
using MediatR;
using OrdersService.Domain.Dtos;

namespace OrdersService.Application.Features.Orders.GetPagedOrders;

public record GetPagedOrdersQuery(
    OrderFilter OrderFilter,
    SortParams SortParams,
    PageParams PageParams) : IRequest<PagedList<OrderDto>>;