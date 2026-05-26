namespace NotificationService.Domain.Interfaces;

public interface ITransactionCoordinatorFactory
{
    ITransactionCoordinator GetCoordinator<T>() where T : ITransactionCoordinator;
}