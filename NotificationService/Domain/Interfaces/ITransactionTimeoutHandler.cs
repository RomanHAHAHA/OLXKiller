namespace NotificationService.Domain.Interfaces;

public interface ITransactionTimeoutHandler
{
    Task<bool> TryHandleTimeoutAsync(
        ITransactionCoordinator coordinator,
        Guid correlationId,
        Func<Task> onTimeout,
        CancellationToken cancellationToken);
}