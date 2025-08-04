using Common.Infrastructure.Messaging.Events.User;
using MassTransit;
using MediatR;
using OrdersService.Application.Features.Users.UpdateAvatar;

namespace OrdersService.Infrastructure.Messaging.Consumers;

public class UserAvatarUpdatedConsumer(IMediator mediator) : IConsumer<UserAvatarUpdatedEvent>
{
    public async Task Consume(ConsumeContext<UserAvatarUpdatedEvent> context)
    {
        var @event = context.Message;
        var command = new UpdateUserAvatarCommand(
            @event.CorrelationId,
            @event.UserId, 
            @event.AvatarPath);
        
        await mediator.Send(command, context.CancellationToken);
    }
}