using MediatR;
using Microsoft.AspNetCore.SignalR;
using NotificationService.API.Hubs;
using NotificationService.Domain.Interfaces;
using NotificationService.Domain.Models;
using NotificationService.Domain.TransactionCoordinators;

namespace NotificationService.Application.Features.Product.NotifyProductCreated;

public class NotifyProductCreatedCommandHandler(
    ITransactionCompleter completer,
    ITransactionCoordinatorFactory coordinatorFactory,
    IHubContext<NotificationHub, INotificationClient> hubContext) : IRequestHandler<NotifyProductSnapshotsCreatedCommand>
{
    public async Task Handle(NotifyProductSnapshotsCreatedCommand request, CancellationToken cancellationToken)
    {
        var transactionCoordinator = coordinatorFactory.GetCoordinator<ProductCreationCoordinator>();
        var transactionDetails = new TransactionDetails(request.CorrelationId, request.SenderServiceName);
        
        await completer.TryCompleteAsync(
            transactionCoordinator,
            transactionDetails,
            async () =>
            {
                await hubContext.Clients
                    .User(request.UserId.ToString())
                    .NotifyProductCreated(request.ProductId, "Product successfully created");
            },
            cancellationToken);
    }
}