using Common.Domain.Enums;
using OrdersService.Application.Features.OrderStatusNotifications.Strategies;

namespace OrdersService.Application.Features.OrderStatusNotifications;

public class OrderStatusNotificationStrategyFactory : IOrderStatusNotificationStrategyFactory
{
    public IOrderStatusNotificationStrategy? CreateStrategy(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Confirmed => new OrderPlacedNotificationStrategy(),
            OrderStatus.Shipped => new OrderShippedNotificationStrategy(),
            OrderStatus.Delivered => new OrderDeliveredNotificationStrategy(),
            OrderStatus.Payed => new OrderPayedNotificationStrategy(),
            OrderStatus.Canceled => new OrderCanceledNotificationStrategy(),
            OrderStatus.Failed => new OrderFailedNotificationStrategy(),
            _ => null,
        };
    }
}