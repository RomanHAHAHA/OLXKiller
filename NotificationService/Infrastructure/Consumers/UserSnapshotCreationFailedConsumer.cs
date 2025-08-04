using Common.Infrastructure.Messaging.Events.User;
using MassTransit;
using MediatR;
using NotificationService.Application.Features.User.NotifyUserRegistrationFailed;

namespace NotificationService.Infrastructure.Consumers;

public class UserSnapshotCreationFailedConsumer(
    IMediator mediator) : IConsumer<UserSnapshotCreationFailedEvent>
{
    public async Task Consume(ConsumeContext<UserSnapshotCreationFailedEvent> context)
    {
        var @event = context.Message;
        var command = new NotifyUserRegistrationFailedCommand(
            @event.CorrelationId,
            @event.ConnectionId);
        
        await mediator.Send(command, context.CancellationToken);
    }
}