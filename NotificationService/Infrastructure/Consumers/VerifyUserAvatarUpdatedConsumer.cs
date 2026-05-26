using Common.Infrastructure.Messaging.Events.User;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.API.Hubs;
using NotificationService.Domain.Interfaces;
using NotificationService.Domain.TransactionCoordinators;

namespace NotificationService.Infrastructure.Consumers;

public class VerifyUserAvatarUpdatedConsumer(
    IPublishEndpoint publisher,
    ITransactionCoordinatorFactory coordinatorFactory,
    ITransactionTimeoutHandler timeoutHandler,
    IHubContext<NotificationHub, INotificationClient> hubContext) : IConsumer<VerifyUserAvatarUpdatedEvent>
{
    public async Task Consume(ConsumeContext<VerifyUserAvatarUpdatedEvent> context)
    {
        var @event = context.Message;
        var coordinator = coordinatorFactory.GetCoordinator<AvatarUpdateCoordinator>();
        
        await timeoutHandler.TryHandleTimeoutAsync(
            coordinator,
            @event.CorrelationId,
            async () =>
            {
                await OnTimeout(@event, context);
            },
            context.CancellationToken);
    }
    
    private async Task OnTimeout(
        VerifyUserAvatarUpdatedEvent @event,
        ConsumeContext<VerifyUserAvatarUpdatedEvent> context)
    {
        await hubContext.Clients
            .User(context.Message.UserId.ToString())
            .NotifyUserAvatarUpdateFailed("Avatar update timed out! Try again later.");
        
        await publisher.Publish(new UserAvatarUpdateTimeoutEvent
        {
            CorrelationId = @event.CorrelationId,
            UserId = @event.UserId,
            SenderServiceName = @event.SenderServiceName,
        }, context.CancellationToken);
    }
}