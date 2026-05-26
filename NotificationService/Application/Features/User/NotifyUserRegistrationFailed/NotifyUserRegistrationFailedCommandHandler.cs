using MediatR;
using Microsoft.AspNetCore.SignalR;
using NotificationService.API.Hubs;
using NotificationService.Domain.Interfaces;
using NotificationService.Domain.TransactionCoordinators;

namespace NotificationService.Application.Features.User.NotifyUserRegistrationFailed;

public class NotifyUserRegistrationFailedCommandHandler(
    ITransactionFailureHandler failureHandler,
    ITransactionCoordinatorFactory coordinatorFactory,
    IHubContext<NotificationHub, INotificationClient> hubContext) : IRequestHandler<NotifyUserRegistrationFailedCommand>
{
    public async Task Handle(NotifyUserRegistrationFailedCommand request, CancellationToken cancellationToken)
    {
        var coordinator = coordinatorFactory.GetCoordinator<UserRegisterCoordinator>();

        await failureHandler.TryHandleFailureAsync(
            coordinator,
            request.CorrelationId,
            async () =>
            {
                await OnFailure(request);
            });
    }

    private async Task OnFailure(NotifyUserRegistrationFailedCommand request)
    {
        await hubContext.Clients
            .Client(request.ConnectionId)
            .NotifyUserRegistrationFailed();
    }
}