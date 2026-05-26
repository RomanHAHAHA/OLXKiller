namespace NotificationService.Domain.Interfaces;

public interface ITransactionFailureHandler
{
    Task<bool> TryHandleFailureAsync(
        ITransactionCoordinator coordinator,
        Guid correlationId,
        Func<Task> onFailure);
}