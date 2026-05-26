using Common.Infrastructure.Messaging.Events.User;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.API.Hubs;
using NotificationService.Domain.Interfaces;
using NotificationService.Domain.TransactionCoordinators;

namespace NotificationService.Infrastructure.Consumers;

public class VerifyUserRegisteredConsumer(
    IPublishEndpoint publisher,
    ITransactionCoordinatorFactory coordinatorFactory,
    ITransactionTimeoutHandler timeoutHandler,
    IHubContext<NotificationHub, INotificationClient> hubContext) : IConsumer<VerifyUserRegisteredEvent>
{
    public async Task Consume(ConsumeContext<VerifyUserRegisteredEvent> context)
    {
        var @event = context.Message;
        var coordinator = coordinatorFactory.GetCoordinator<UserRegisterCoordinator>();
        
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
        VerifyUserRegisteredEvent @event,
        ConsumeContext<VerifyUserRegisteredEvent> context)
    {
        await hubContext.Clients
            .Clients(@event.ConnectionId)
            .NotifyUserRegistrationFailed();
        
        await publisher.Publish(new UserRegisterTimeoutEvent
        {
            CorrelationId = @event.CorrelationId,
            UserId = @event.UserId,
            ConnectionId =  @event.ConnectionId,
            SenderServiceName = @event.SenderServiceName,
        }, context.CancellationToken);
    }
}