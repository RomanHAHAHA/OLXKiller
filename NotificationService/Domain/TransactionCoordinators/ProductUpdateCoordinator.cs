using Common.Domain.Constants;
using NotificationService.Domain.Interfaces;

namespace NotificationService.Domain.TransactionCoordinators;

public class ProductUpdateCoordinator : ITransactionCoordinator
{
    public string[] RequiredServices => 
    [
        ProductCreationRequiredServices.CartsService,
        ProductCreationRequiredServices.OrdersService,
        ProductCreationRequiredServices.ReviewsService
    ];
    
    public string GetLockKey(Guid id) => $"lock:product-update:{id}";
    
    public string GetCompletedKey(Guid id) => $"product-update-completed:{id}";
}