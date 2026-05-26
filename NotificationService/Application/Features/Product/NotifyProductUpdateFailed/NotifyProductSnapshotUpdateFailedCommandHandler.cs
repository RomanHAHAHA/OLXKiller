using MediatR;
using Microsoft.AspNetCore.SignalR;
using NotificationService.API.Hubs;
using NotificationService.Domain.Interfaces;
using NotificationService.Domain.TransactionCoordinators;

namespace NotificationService.Application.Features.Product.NotifyProductUpdateFailed;

public class NotifyProductSnapshotUpdateFailedCommandHandler(
    ITransactionFailureHandler failureHandler,
    ITransactionCoordinatorFactory coordinatorFactory,
    IHubContext<NotificationHub, INotificationClient> hubContext) : IRequestHandler<NotifyProductSnapshotUpdateFailedCommand>
{
    public async Task Handle(NotifyProductSnapshotUpdateFailedCommand request, CancellationToken cancellationToken)
    {
        var coordinator = coordinatorFactory.GetCoordinator<ProductUpdateCoordinator>();
        
        await failureHandler.TryHandleFailureAsync(
            coordinator, 
            request.CorrelationId,
            async () =>
            {
                await OnFailure(request);
            });
    }

    private async Task OnFailure(NotifyProductSnapshotUpdateFailedCommand request)
    {
        await hubContext.Clients
            .User(request.UserId.ToString())
            .NotifyProductUpdateFailed("Unexpected server error occured during the request. Please try again later.");
    }
}