using MediatR;
using Microsoft.AspNetCore.SignalR;
using NotificationService.API.Hubs;
using NotificationService.Domain.Interfaces;
using NotificationService.Domain.Models;
using NotificationService.Domain.TransactionCoordinators;

namespace NotificationService.Application.Features.Product.NotifyProductUpdated;

public class NotifyProductUpdatedCommandHandler(
    ITransactionCompleter transactionCompleter,
    ITransactionCoordinatorFactory coordinatorFactory,
    IHubContext<NotificationHub, INotificationClient> hubContext) : IRequestHandler<NotifyProductUpdatedCommand>
{
    public async Task Handle(NotifyProductUpdatedCommand request, CancellationToken cancellationToken)
    {
        var transactionCoordinator = coordinatorFactory.GetCoordinator<ProductCreationCoordinator>();
        var transactionDetails = new TransactionDetails(request.CorrelationId, request.SenderServiceName);
        
        await transactionCompleter.TryCompleteAsync(
            transactionCoordinator,
            transactionDetails,
            async () =>
            {
                await OnCompleted(request);
            },
            cancellationToken);
    }

    private async Task OnCompleted(NotifyProductUpdatedCommand request)
    {
        await hubContext.Clients
            .User(request.UserId.ToString())
            .NotifyProductUpdated(request.ProductId, "Product successfully updated");
    }
}