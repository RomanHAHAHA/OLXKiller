using Common.Infrastructure.Messaging.Events.User;
using MassTransit;
using MediatR;
using NotificationService.Application.Features.User.NotifyUserRegistered;

namespace NotificationService.Infrastructure.Consumers;

public class UserSnapshotCreatedConsumer(
    IMediator mediator) : IConsumer<UserSnapshotCreatedEvent>
{
    public async Task Consume(ConsumeContext<UserSnapshotCreatedEvent> context)
    {
        var @event = context.Message;
        var command = new NotifyUserRegisteredCommand(
            @event.CorrelationId,
            @event.SenderServiceName,
            @event.ConnectionId);
        
        await mediator.Send(command, context.CancellationToken);
    }
}