using System.Text;
using OrdersService.Domain.Entities;

namespace OrdersService.Application.Features.OrderStatusNotifications.Strategies;

public class OrderPayedNotificationStrategy : IOrderStatusNotificationStrategy
{
    public string BuildSubject(Guid orderId) => $"Payment Confirmed for Order #{orderId}";
    
    public string BuildContent(Order order)
    {
        var totalPrice = order.OrderItems.Sum(oi => oi.Product?.Price * oi.Quantity);
        
        var sb = new StringBuilder();
        sb.AppendLine("<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; color: #333; line-height: 1.6;'>");
        sb.AppendLine($"<h1 style='color: #2c3e50;'>Payment Received</h1>");
        sb.AppendLine($"<p>We've successfully processed your payment for order #{order.Id}.</p>");
        sb.AppendLine($"<p><strong>Amount:</strong> {totalPrice:C}</p>");
        sb.AppendLine("<p>Your order is now being prepared for shipment.</p></div>");
        return sb.ToString();
    }
}