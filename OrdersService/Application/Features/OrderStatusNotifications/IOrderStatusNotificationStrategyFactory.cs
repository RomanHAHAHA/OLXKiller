using Common.Domain.Enums;
using OrdersService.Application.Features.OrderStatusNotifications.Strategies;

namespace OrdersService.Application.Features.OrderStatusNotifications;

public interface IOrderStatusNotificationStrategyFactory
{
    IOrderStatusNotificationStrategy? CreateStrategy(OrderStatus status);
}