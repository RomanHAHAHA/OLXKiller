using System.Text;
using OrdersService.Domain.Entities;

namespace OrdersService.Application.Features.OrderStatusNotifications.Strategies;

public class OrderCanceledNotificationStrategy : IOrderStatusNotificationStrategy
{
    public string BuildSubject(Guid orderId) => $"Order #{orderId} Cancellation";
    
    public string BuildContent(Order order)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; color: #333; line-height: 1.6;'>");
        sb.AppendLine($"<h1 style='color: #2c3e50;'>Order Cancellation</h1>");
        sb.AppendLine($"<p>Your order #{order.Id} has been cancelled as requested.</p>");
        
        sb.AppendLine("<div style='background: #ffebee; padding: 15px; margin: 20px 0;'>");
        sb.AppendLine("<h3 style='color: #2c3e50; font-size: 16px;'>Refund Information</h3>");
        sb.AppendLine("<p>If applicable, your refund will be processed within 5-7 business days.</p>");
        sb.AppendLine("</div>");
        
        sb.AppendLine("<p>We're sorry to see you go. Hope to see you again!</p></div>");
        return sb.ToString();
    }
}