using Common.Domain.Constants;
using NotificationService.Domain.Interfaces;

namespace NotificationService.Domain.TransactionCoordinators;

public class AvatarUpdateCoordinator : ITransactionCoordinator
{
    public string[] RequiredServices => 
    [
        AvatarUpdateRequiredServices.OrdersService,
        AvatarUpdateRequiredServices.ReviewsService,
        AvatarUpdateRequiredServices.ChatsService,
    ];
    
    public string GetLockKey(Guid id) => $"lock:avatar-update:{id}";
    
    public string GetCompletedKey(Guid id) => $"avatar-update-completed:{id}";
}