using NotificationService.Application.Services;
using NotificationService.Domain.Models;

namespace NotificationService.Domain.Interfaces;

public interface ITransactionCompleter
{
    Task<bool> TryCompleteAsync(
        ITransactionCoordinator transactionCoordinator,
        TransactionDetails transactionDetails,
        Func<Task> onComplete,
        CancellationToken cancellationToken);
}