using System.Text;
using OrdersService.Domain.Entities;

namespace OrdersService.Application.Features.OrderStatusNotifications.Strategies;

public class OrderShippedNotificationStrategy : IOrderStatusNotificationStrategy
{
    public string BuildSubject(Guid orderId) => $"Your Order #{orderId} Has Shipped!";
    
    public string BuildContent(Order order)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; color: #333; line-height: 1.6;'>");
        sb.AppendLine($"<h1 style='color: #2c3e50;'>Your Order is On the Way!</h1>");
        sb.AppendLine($"<p>Order #{order.Id} has been shipped and will arrive soon.</p>");
        
        if (order.DeliveryLocation is not null)
        {
            sb.AppendLine("<h2 style='color: #2c3e50; font-size: 18px;'>Delivery Details</h2>");
            sb.AppendLine($"<p><strong>Estimated Delivery:</strong> 3-5 business days</p>");
            sb.AppendLine($"<p><strong>Delivery Point:</strong> {order.DeliveryLocation.Warehouse}, {order.DeliveryLocation.City}</p>");
        }
        
        sb.AppendLine("<div style='background: #f8f9fa; padding: 15px; margin-top: 20px;'>");
        sb.AppendLine("<h3 style='color: #2c3e50; font-size: 16px;'>Track Your Order</h3>");
        sb.AppendLine("<p>Use <a href='[https://localhost:3000/profile/my-orders]' style='color: #3498db;'>this link</a> to track your package.</p>");
        sb.AppendLine("</div></div>");
        
        return sb.ToString();
    }
}