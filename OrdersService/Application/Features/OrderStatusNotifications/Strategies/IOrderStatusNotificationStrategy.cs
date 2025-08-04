using OrdersService.Domain.Entities;

namespace OrdersService.Application.Features.OrderStatusNotifications.Strategies;

public interface IOrderStatusNotificationStrategy
{
    string BuildSubject(Guid orderId);
    
    string BuildContent(Order order);
}