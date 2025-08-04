using Common.Application.Options;
using Common.Domain.Enums;
using Common.Domain.Models.Results;
using Common.Infrastructure.Messaging.Events.Order;
using Common.Infrastructure.Messaging.Events.SystemAction;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OrdersService.Application.Features.OrderStatusNotifications;
using OrdersService.Domain.Entities;
using OrdersService.Infrastructure.Persistence;

namespace OrdersService.Application.Features.Orders.SetStatus;

public class SetOrderStatusCommandHandler(
    OrdersDbContext dbContext,
    IPublishEndpoint publisher,
    IOrderStatusNotificationStrategyFactory strategyFactory,
    IOptions<ServiceOptions> serviceOptions) : IRequestHandler<SetOrderStatusCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(SetOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders
            .Include(o => o.User)
            .Include(o => o.DeliveryLocation)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
        {
            return ApiResponse.NotFound(nameof(Order));
        }
        
        order.ChangeStatus(request.OrderStatus);
        await OnOrderStatusSet(order, request.InitiatorUserId, cancellationToken);
        
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return ApiResponse.Ok();
    }

    private async Task OnOrderStatusSet(Order order, Guid initiatorId, CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid();
        var serviceName = serviceOptions.Value.Name;
        
        await publisher.Publish(
            new SystemActionEvent
            {
                CorrelationId = correlationId,
                SenderServiceName = serviceName,
                UserId = initiatorId,
                ActionType = ActionType.Update,
                Message = $"Order {order.Id} status set {order.CurrentStatus}"
            }, 
            cancellationToken);
        
        var strategy = strategyFactory.CreateStrategy(order.CurrentStatus);
       
        if (strategy is null)
        {
            return;
        }
        
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
}