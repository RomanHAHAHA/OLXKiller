using Common.Infrastructure.Messaging.Events.User;
using MassTransit;
using MediatR;
using OrdersService.Application.Features.Users.RollBackAvatar;

namespace OrdersService.Infrastructure.Messaging.Consumers;

public class UserAvatarRollBackConsumer(IMediator mediator) : IConsumer<UserAvatarRollbackEvent>
{
    public async Task Consume(ConsumeContext<UserAvatarRollbackEvent> context)
    {
        var @event = context.Message;
        var command = new RollBackUserAvatarCommand(@event.UserId, @event.PreviousAvatarName);
        await mediator.Send(command, context.CancellationToken);
    }
}