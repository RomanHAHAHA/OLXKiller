using ChatsService.Application.Features.Users.RollBackAvatar;
using Common.Infrastructure.Messaging.Events.User;
using MassTransit;
using MediatR;

namespace ChatsService.Infrastructure.Consumers;

public class UserAvatarRollBackConsumer(IMediator mediator) : IConsumer<UserAvatarRollbackEvent>
{
    public async Task Consume(ConsumeContext<UserAvatarRollbackEvent> context)
    {
        var @event = context.Message;
        var command = new RollBackUserAvatarCommand(@event.UserId, @event.PreviousAvatarName);
        await mediator.Send(command, context.CancellationToken);
    }
}