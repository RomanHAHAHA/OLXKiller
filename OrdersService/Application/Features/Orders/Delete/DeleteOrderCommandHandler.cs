using MediatR;
using OrdersService.Domain.Interfaces;

namespace OrdersService.Application.Features.Orders.Delete;

public class DeleteOrderCommandHandler(
    IOrdersRepository ordersRepository,
    ILogger<DeleteOrderCommandHandler> logger) : IRequestHandler<DeleteOrderCommand>
{
    public async Task Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await ordersRepository.GetByIdAsync(request.OrderId, cancellationToken);

        if (order is null)
        {
            logger.LogInformation($"Order {request.OrderId} was not found to delete");
            return;
        }
        
        ordersRepository.Delete(order);
        await ordersRepository.SaveChangesAsync(cancellationToken);
    }
}