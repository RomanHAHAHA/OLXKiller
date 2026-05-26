namespace NotificationService.Domain.Interfaces;

public interface ITransactionCoordinator
{
    string[] RequiredServices { get; }
    
    string GetLockKey(Guid id);

    string GetCompletedKey(Guid id);
}