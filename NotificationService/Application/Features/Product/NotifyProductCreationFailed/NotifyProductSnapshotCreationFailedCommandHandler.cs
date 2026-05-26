using MediatR;
using Microsoft.AspNetCore.SignalR;
using NotificationService.API.Hubs;
using NotificationService.Domain.Interfaces;
using NotificationService.Domain.TransactionCoordinators;

namespace NotificationService.Application.Features.Product.NotifyProductCreationFailed;

public class NotifyProductSnapshotCreationFailedCommandHandler(
    ITransactionCoordinatorFactory coordinatorFactory,
    ITransactionFailureHandler failureHandler,
    IHubContext<NotificationHub, INotificationClient> hubContext) : IRequestHandler<NotifyProductSnapshotCreationFailedCommand>
{
    public async Task Handle(NotifyProductSnapshotCreationFailedCommand request, CancellationToken cancellationToken)
    {
        var coordinator = coordinatorFactory.GetCoordinator<ProductCreationCoordinator>();
        
        await failureHandler.TryHandleFailureAsync(
            coordinator,
            request.CorrelationId,
            async () =>
            {
                await hubContext.Clients
                    .User(request.UserId.ToString())
                    .NotifyProductCreationFailed("Unexpected server error occurred during the request. Please try again later.");
            });
    }
}