using Common.Domain.Abstractions;

namespace Common.Infrastructure.Messaging.Events.Order;

public class OrderStatusChangedEvent : BaseEvent
{   
    public string UserEmail { get; init; } = string.Empty;
    
    public string EmailSubject { get; init; } = string.Empty;
    
    public string EmailContent { get; init; } = string.Empty;
}