using Common.Infrastructure.Messaging.Events.Product;

namespace NotificationService.Domain.Interfaces;

public interface INotificationClient
{
    Task NotifyProductCreated(Guid productId, string message);
    
    Task NotifyProductCreationFailed(string error);
    
    Task NotifyProductUpdated(Guid productId, string message);

    Task NotifyProductUpdateFailed(string error);
    
    Task NotifyUserRegistered();
    
    Task NotifyUserRegistrationFailed();
    
    Task NotifyUserAvatarUpdated(string message);

    Task NotifyUserAvatarUpdateFailed(string message);
    
    Task NotifyOrderProcessed(string message);
    
    Task NotifyProductsReservationFailed(List<ProductStockInfo> productStockInfos);
    
    Task NotifyProductStockExceeded(int stockQuantity);
}