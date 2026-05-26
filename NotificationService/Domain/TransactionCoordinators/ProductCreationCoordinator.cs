using Common.Domain.Constants;
using NotificationService.Domain.Interfaces;

namespace NotificationService.Domain.TransactionCoordinators;

public class ProductCreationCoordinator : ITransactionCoordinator
{
    public string[] RequiredServices => 
    [
        ProductCreationRequiredServices.CartsService,
        ProductCreationRequiredServices.OrdersService,
        ProductCreationRequiredServices.ReviewsService
    ];
    
    public string GetLockKey(Guid id) => $"lock:product-creation:{id}";
    
    public string GetCompletedKey(Guid id) => $"product-creation-completed:{id}";
}