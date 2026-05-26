using MediatR;
using Microsoft.AspNetCore.SignalR;
using NotificationService.API.Hubs;
using NotificationService.Domain.Interfaces;
using NotificationService.Domain.TransactionCoordinators;

namespace NotificationService.Application.Features.User.NotifyAvatarUpdateFailure;

public class NotifyAvatarUpdateFailedCommandHandler(
    ITransactionFailureHandler failureHandler,
    ITransactionCoordinatorFactory coordinatorFactory,
    IHubContext<NotificationHub, INotificationClient> hubContext) : IRequestHandler<NotifyAvatarUpdateFailedCommand>
{
    public async Task Handle(NotifyAvatarUpdateFailedCommand request, CancellationToken cancellationToken)
    {
        var coordinator = coordinatorFactory.GetCoordinator<AvatarUpdateCoordinator>();
        
        await failureHandler.TryHandleFailureAsync(
            coordinator, 
            request.CorrelationId,
            async () =>
            {
               await OnFailure(request);
            });
    }

    private async Task OnFailure(NotifyAvatarUpdateFailedCommand request)
    {
        await hubContext.Clients
            .User(request.UserId.ToString())
            .NotifyUserAvatarUpdateFailed("Unexpected server error occured during the request. Please try again later.");
    }
}