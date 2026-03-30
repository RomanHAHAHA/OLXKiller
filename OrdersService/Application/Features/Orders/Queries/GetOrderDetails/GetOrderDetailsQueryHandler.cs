using Common.Domain.Models.Results;
using MediatR;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Interfaces;

namespace OrdersService.Application.Features.Orders.Queries.GetOrderDetails;

public class GetOrderDetailsQueryHandler(
    IOrdersRepository ordersRepository) : IRequestHandler<GetOrderDetailsQuery, ApiResponse<OrderDetailsDto>>
{
    public async Task<ApiResponse<OrderDetailsDto>> Handle(GetOrderDetailsQuery request, CancellationToken cancellationToken)
    {
        var orderDto = await ordersRepository.GetOrderDetailsAsync(request.OrderId, cancellationToken);

        return orderDto is null
            ? ApiResponse<OrderDetailsDto>.NotFound(nameof(Order))
            : ApiResponse<OrderDetailsDto>.Ok(orderDto);
    }
}