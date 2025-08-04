using Common.Domain.Enums;
using MediatR;

namespace OrdersService.Application.Features.Orders.GetAllStatuses;

public class GetAllOrderStatusesQueryHandler : 
    IRequestHandler<GetAllOrderStatusesQuery, List<DbOrderStatusDto>>
{
    public Task<List<DbOrderStatusDto>> Handle(
        GetAllOrderStatusesQuery request, 
        CancellationToken cancellationToken)
    {
        var statuses = Enum.GetValues<OrderStatus>()
            .Select(e => new DbOrderStatusDto((int)e, e.ToString()))
            .ToList();
        
        return Task.FromResult(statuses);
    }
}