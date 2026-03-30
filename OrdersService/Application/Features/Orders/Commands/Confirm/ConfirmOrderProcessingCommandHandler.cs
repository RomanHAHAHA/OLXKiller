using Common.Application.Options;
using Common.Domain.Enums;
using Common.Infrastructure.Messaging.Events.Order;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OrdersService.Application.Features.OrderStatusNotifications;
using OrdersService.Domain.Entities;
using OrdersService.Infrastructure.Persistence;

namespace OrdersService.Application.Features.Orders.Commands.Confirm;

public class ConfirmOrderProcessingCommandHandler(
    OrdersDbContext dbContext,
    IPublishEndpoint publisher,
    IOrderStatusNotificationStrategyFactory strategyFactory,
    IOptions<ServiceOptions> serviceOptions) : IRequestHandler<ConfirmOrderProcessingCommand>
{
    public async Task Handle(ConfirmOrderProcessingCommand request, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders
            .Include(o => o.User)
            .Include(o => o.DeliveryLocation)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
        {
            return;
        }

        order.ChangeStatus(OrderStatus.Confirmed);
        
        try
        {
            await OnOrderProcessed(order, cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private async Task OnOrderProcessed(Order order, CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid();
        var serviceName = serviceOptions.Value.Name;
        
        await publisher.Publish(
            new OrderProcessedEvent
            {
                CorrelationId = correlationId,
                SenderServiceName = serviceName,
                UserId = order.UserId,
            }, 
            cancellationToken);

        
        var strategy = strategyFactory.CreateStrategy(OrderStatus.Confirmed);
       
        if (strategy is not null)
        {
            await publisher.Publish(
                new OrderStatusChangedEvent
                {
                    CorrelationId = correlationId,
                    SenderServiceName = serviceName,
                    UserEmail = order.User?.Email ?? string.Empty,
                    EmailSubject = strategy.BuildSubject(order.Id),
                    EmailContent = strategy.BuildContent(order),
                },
                cancellationToken);
        }
        
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}