using ChatsService.Application.Features.Users.UpdateAvatar;
using Common.Infrastructure.Messaging.Events.User;
using MassTransit;
using MediatR;

namespace ChatsService.Infrastructure.Consumers;

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