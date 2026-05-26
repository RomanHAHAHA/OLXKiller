using Common.Domain.Constants;
using NotificationService.Domain.Interfaces;

namespace NotificationService.Domain.TransactionCoordinators;

public class UserRegisterCoordinator : ITransactionCoordinator
{
    public string[] RequiredServices =>
    [
        UserRegistrationRequiredServices.ReviewsService,
        UserRegistrationRequiredServices.OrdersService,
        UserRegistrationRequiredServices.ChatsService,
    ];
    
    public string GetLockKey(Guid id) => $"lock:user-register:{id}";
    
    public string GetCompletedKey(Guid id) => $"user-register-completed:{id}";
}