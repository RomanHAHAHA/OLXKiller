using System.Text;
using OrdersService.Domain.Entities;

namespace OrdersService.Application.Features.OrderStatusNotifications.Strategies;

public class OrderDeliveredNotificationStrategy : IOrderStatusNotificationStrategy
{
    public string BuildSubject(Guid orderId) => $"Order #{orderId} Has Been Delivered";
    
    public string BuildContent(Order order)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; color: #333; line-height: 1.6;'>");
        sb.AppendLine("<h1 style='color: #2c3e50;'>Your Order Has Arrived!</h1>");
        sb.AppendLine($"<p>Order #{order.Id} has been successfully delivered.</p>");
        
        sb.AppendLine("<div style='background: #e8f5e9; padding: 15px; margin: 20px 0;'>");
        sb.AppendLine("<h3 style='color: #2c3e50; font-size: 16px;'>Next Steps</h3>");
        sb.AppendLine("<ul style='padding-left: 20px;'>");
        sb.AppendLine("<li>Check all items in your package</li>");
        sb.AppendLine("<li>Contact support within 7 days for any issues</li>");
        sb.AppendLine("</ul></div>");
        
        sb.AppendLine("<p>We hope you enjoy your purchase!</p></div>");
        return sb.ToString();
    }
}