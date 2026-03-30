using MediatR;
using OrdersService.Domain.Dtos;
using OrdersService.Domain.Interfaces;

namespace OrdersService.Application.Features.Orders.Queries.GetUserOrders;

public class GetUserOrdersCommandHandler(
    IOrdersRepository ordersRepository) : IRequestHandler<GetUserOrdersCommand, List<PersonalOrderDto>>
{
    public async Task<List<PersonalOrderDto>> Handle(
        GetUserOrdersCommand request, 
        CancellationToken cancellationToken)
    {
        return await ordersRepository
            .GetPersonalOrdersAsync(request.UserId, cancellationToken);
    }
}