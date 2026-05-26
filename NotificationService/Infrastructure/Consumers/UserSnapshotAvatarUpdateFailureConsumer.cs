using Common.Infrastructure.Messaging.Events.User;
using MassTransit;
using MediatR;
using NotificationService.Application.Features.User.NotifyAvatarUpdateFailure;

namespace NotificationService.Infrastructure.Consumers;

public class UserSnapshotAvatarUpdateFailureConsumer(IMediator mediator) : IConsumer<UserAvatarUpdateFailureEvent>
{
    public async Task Consume(ConsumeContext<UserAvatarUpdateFailureEvent> context)
    {
        var @event = context.Message;
        var command = new NotifyAvatarUpdateFailedCommand(
            @event.CorrelationId,
            @event.UserId);
        
        await mediator.Send(command, context.CancellationToken);
    }
}