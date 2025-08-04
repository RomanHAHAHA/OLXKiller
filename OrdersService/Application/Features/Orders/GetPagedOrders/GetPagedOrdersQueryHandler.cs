using Common.Domain.Models.Results;
using MediatR;
using OrdersService.Domain.Dtos;
using OrdersService.Domain.Interfaces;

namespace OrdersService.Application.Features.Orders.GetPagedOrders;

public class GetPagedOrdersQueryHandler(
    IOrdersRepository ordersRepository) : IRequestHandler<GetPagedOrdersQuery, PagedList<OrderDto>>
{
    public async Task<PagedList<OrderDto>> Handle(
        GetPagedOrdersQuery request, 
        CancellationToken cancellationToken)
    {
        return await ordersRepository.GetPagedOrdersAsync(
            request.OrderFilter,
            request.SortParams,
            request.PageParams,
            cancellationToken);
    }
}