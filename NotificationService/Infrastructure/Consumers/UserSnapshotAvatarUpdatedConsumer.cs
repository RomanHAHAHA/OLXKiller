
using Common.Infrastructure.Messaging.Events.User;
using MassTransit;
using MediatR;
using NotificationService.Application.Features.User.NotifyAvatarUpdated;

namespace NotificationService.Infrastructure.Consumers;

public class UserSnapshotAvatarUpdatedConsumer(IMediator mediator) : IConsumer<UserSnapshotAvatarUpdatedEvent>
{
    public async Task Consume(ConsumeContext<UserSnapshotAvatarUpdatedEvent> context)
    {
        var @event = context.Message;
        var command = new NotifyAvatarUpdatedCommand(
            @event.CorrelationId,
            @event.SenderServiceName,
            @event.UserId);
        
        await mediator.Send(command, context.CancellationToken);
    }
}