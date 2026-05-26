using NotificationService.Domain.Interfaces;

namespace NotificationService.Application.Services;

public class TransactionCoordinatorFactory(IServiceProvider serviceProvider) : ITransactionCoordinatorFactory
{
    public ITransactionCoordinator GetCoordinator<T>() where T : ITransactionCoordinator
    {
        return serviceProvider.GetRequiredService<T>();
    }
}