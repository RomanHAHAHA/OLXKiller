using System.Text;
using OrdersService.Domain.Entities;

namespace OrdersService.Application.Features.OrderStatusNotifications.Strategies;

public class OrderFailedNotificationStrategy : IOrderStatusNotificationStrategy
{
    public string BuildSubject(Guid orderId) => $"Issue with Your Order #{orderId}";
    
    public string BuildContent(Order order)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; color: #333; line-height: 1.6;'>");
        sb.AppendLine($"<h1 style='color: #d32f2f;'>Order Processing Issue</h1>");
        sb.AppendLine($"<p>We encountered an issue processing your order #{order.Id}.</p>");
        
        sb.AppendLine("<div style='background: #fff3e0; padding: 15px; margin: 20px 0;'>");
        sb.AppendLine("<h3 style='color: #2c3e50; font-size: 16px;'>Next Steps</h3>");
        sb.AppendLine("<ul style='padding-left: 20px;'>");
        sb.AppendLine("<li>Our team is working to resolve the issue</li>");
        sb.AppendLine("<li>You'll receive an update within 24 hours</li>");
        sb.AppendLine("<li>No further action is required from you at this time</li>");
        sb.AppendLine("</ul></div>");
        
        sb.AppendLine("<p>We apologize for the inconvenience.</p></div>");
        return sb.ToString();
    }
}