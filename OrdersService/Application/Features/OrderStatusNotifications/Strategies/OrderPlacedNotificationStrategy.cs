using System.Text;
using OrdersService.Domain.Entities;

namespace OrdersService.Application.Features.OrderStatusNotifications.Strategies;

public class OrderPlacedNotificationStrategy : IOrderStatusNotificationStrategy
{
    public string BuildSubject(Guid orderId) => $"Order Confirmation #{orderId}";
    
    public string BuildContent(Order order)
    {
        var totalPrice = order.OrderItems.Sum(oi => oi.FixedPrice * oi.Quantity);
        
        var sb = new StringBuilder();
        
        sb.AppendLine("<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; color: #333; line-height: 1.6;'>");
        
        sb.AppendLine($"<h1 style='color: #2c3e50; margin-bottom: 10px;'>Thank you for your order!</h1>");
        sb.AppendLine($"<p style='font-size: 16px;'>Order number: <strong>#{order.Id}</strong></p>");
        sb.AppendLine("<p style='font-size: 16px; margin-bottom: 20px;'>We've received your order and it's now being processed.</p>");
        
        if (order.DeliveryLocation is not null)
        {
            sb.AppendLine("<div style='margin-bottom: 25px;'>");
            sb.AppendLine("<h2 style='color: #2c3e50; font-size: 18px; margin-bottom: 10px; border-bottom: 1px solid #eee; padding-bottom: 5px;'>Delivery Information</h2>");
            sb.AppendLine("<div style='background: #f8f9fa; padding: 12px 15px; border-radius: 5px;'>");
            sb.AppendLine($"<p style='margin: 5px 0;'><strong style='display: inline-block; width: 80px;'>Region:</strong> {order.DeliveryLocation.Region}</p>");
            sb.AppendLine($"<p style='margin: 5px 0;'><strong style='display: inline-block; width: 80px;'>City:</strong> {order.DeliveryLocation.City}</p>");
            sb.AppendLine($"<p style='margin: 5px 0;'><strong style='display: inline-block; width: 80px;'>Warehouse:</strong> {order.DeliveryLocation.Warehouse}</p>");
            sb.AppendLine("</div></div>");
        }
        
        sb.AppendLine("<h2 style='color: #2c3e50; font-size: 18px; margin-bottom: 10px; border-bottom: 1px solid #eee; padding-bottom: 5px;'>Order Details</h2>");
        sb.AppendLine("<table style='width: 100%; border-collapse: collapse; margin-bottom: 25px;'>");
        
        sb.AppendLine("<thead><tr style='background-color: #f8f9fa;'>");
        sb.AppendLine("<th style='padding: 10px 12px; text-align: left; border-bottom: 1px solid #ddd;'>Product</th>");
        sb.AppendLine("<th style='padding: 10px 12px; text-align: center; border-bottom: 1px solid #ddd; width: 80px;'>Qty</th>");
        sb.AppendLine("<th style='padding: 10px 12px; text-align: right; border-bottom: 1px solid #ddd; width: 100px;'>Price</th>");
        sb.AppendLine("<th style='padding: 10px 12px; text-align: right; border-bottom: 1px solid #ddd; width: 100px;'>Total</th>");
        sb.AppendLine("</tr></thead>");
        
        sb.AppendLine("<tbody>");
        foreach (var item in order.OrderItems)
        {
            var itemTotal = item.FixedPrice * item.Quantity;
            sb.AppendLine("<tr style='border-bottom: 1px solid #eee;'>");
            sb.AppendLine($"<td style='padding: 12px; vertical-align: top;'><strong>{item.Product?.Name}</strong></td>");
            sb.AppendLine($"<td style='padding: 12px; text-align: center; vertical-align: top;'>{item.Quantity}</td>");
            sb.AppendLine($"<td style='padding: 12px; text-align: right; vertical-align: top;'>{item.FixedPrice:C}</td>");
            sb.AppendLine($"<td style='padding: 12px; text-align: right; vertical-align: top;'>{itemTotal:C}</td>");
            sb.AppendLine("</tr>");
        }
        
        sb.AppendLine("<tr>");
        sb.AppendLine("<td colspan='3' style='padding: 12px; text-align: right; border-top: 2px solid #ddd;'><strong>Total:</strong></td>");
        sb.AppendLine($"<td style='padding: 12px; text-align: right; border-top: 2px solid #ddd;'><strong>{totalPrice:C}</strong></td>");
        sb.AppendLine("</tr></tbody></table>");
        
        sb.AppendLine("<div style='background: #f8f9fa; padding: 15px; border-radius: 5px; margin-bottom: 25px;'>");
        sb.AppendLine("<h3 style='color: #2c3e50; font-size: 16px; margin-top: 0; margin-bottom: 10px;'>What happens next?</h3>");
        sb.AppendLine("<ol style='margin: 0; padding-left: 20px;'>");
        sb.AppendLine("<li style='margin-bottom: 8px;'>We'll process your order within 24 hours</li>");
        sb.AppendLine("<li style='margin-bottom: 8px;'>You'll receive a shipping confirmation when your items are dispatched</li>");
        sb.AppendLine("<li>Track your order using our website or mobile app</li>");
        sb.AppendLine("</ol></div>");
        
        sb.AppendLine("<div style='font-size: 14px; color: #777;'>");
        sb.AppendLine("<p style='margin-bottom: 5px;'>If you have any questions, please contact us at:</p>");
        sb.AppendLine("<p style='margin: 0;'><a href='mailto:support@example.com' style='color: #3498db; text-decoration: none;'>support@example.com</a></p>");
        sb.AppendLine("<p style='margin-top: 20px;'>Thank you for shopping with us!</p>");
        sb.AppendLine("</div></div>");

        return sb.ToString();
    }
}