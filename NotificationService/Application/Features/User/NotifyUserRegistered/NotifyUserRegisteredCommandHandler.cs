using MediatR;
using Microsoft.AspNetCore.SignalR;
using NotificationService.API.Hubs;
using NotificationService.Domain.Interfaces;
using NotificationService.Domain.Models;
using NotificationService.Domain.TransactionCoordinators;

namespace NotificationService.Application.Features.User.NotifyUserRegistered;

public class NotifyUserRegisteredCommandHandler(
    ITransactionCompleter completer,
    ITransactionCoordinatorFactory coordinatorFactory,
    IHubContext<NotificationHub, INotificationClient> hubContext) : IRequestHandler<NotifyUserRegisteredCommand>
{
    public async Task Handle(NotifyUserRegisteredCommand request, CancellationToken cancellationToken)
    {
        var coordinator = coordinatorFactory.GetCoordinator<UserRegisterCoordinator>();
        var details = new TransactionDetails(request.CorrelationId, request.SenderServiceName);

        await completer.TryCompleteAsync(
            coordinator,
            details,
            async () =>
            {
                await OnComplete(request);
            },
            cancellationToken);
    }

    private async Task OnComplete(NotifyUserRegisteredCommand request)
    {
        await hubContext.Clients
            .Clients(request.ConnectionId)
            .NotifyUserRegistered();
    }
}