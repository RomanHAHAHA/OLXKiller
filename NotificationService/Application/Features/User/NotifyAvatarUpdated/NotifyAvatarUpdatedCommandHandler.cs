using MediatR;
using Microsoft.AspNetCore.SignalR;
using NotificationService.API.Hubs;
using NotificationService.Domain.Interfaces;
using NotificationService.Domain.Models;
using NotificationService.Domain.TransactionCoordinators;

namespace NotificationService.Application.Features.User.NotifyAvatarUpdated;

public class NotifyAvatarUpdatedCommandHandler(
    ITransactionCompleter completer,
    ITransactionCoordinatorFactory coordinatorFactory,
    IHubContext<NotificationHub, INotificationClient> hubContext) : IRequestHandler<NotifyAvatarUpdatedCommand>
{
    public async Task Handle(NotifyAvatarUpdatedCommand request, CancellationToken cancellationToken)
    {
        var transactionCoordinator = coordinatorFactory.GetCoordinator<AvatarUpdateCoordinator>();
        var transactionDetails = new TransactionDetails(request.CorrelationId, request.SenderServiceName);

        await completer.TryCompleteAsync(
            transactionCoordinator,
            transactionDetails,
            async () =>
            {
                await OnCompletedAsync(request);
            },
            cancellationToken);
    }

    private async Task OnCompletedAsync(NotifyAvatarUpdatedCommand request)
    {
        await hubContext.Clients
            .User(request.UserId.ToString())
            .NotifyUserAvatarUpdated("Avatar successfully updated!");
    }
}