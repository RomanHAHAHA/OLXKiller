using Common.Domain.Models.Results;
using MediatR;
using OrdersService.Domain.Dtos;
using OrdersService.Domain.Interfaces;

namespace OrdersService.Application.Features.Orders.Queries.GetPagedOrders;

public class GetPagedOrdersQueryHandler(
    IOrdersRepository ordersRepository) : IRequestHandler<GetPagedOrdersQuery, PagedList<ShortOrdersDto>>
{
    public async Task<PagedList<ShortOrdersDto>> Handle(GetPagedOrdersQuery request, CancellationToken cancellationToken)
    {
        return await ordersRepository.GetPagedOrdersAsync(
            request.OrderFilter,
            request.SortParams,
            request.PageParams,
            cancellationToken);
    }
}