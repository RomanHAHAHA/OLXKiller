using ChatsService.Application.Features.Users.Delete;
using Common.Infrastructure.Messaging.Events.User;
using MassTransit;
using MediatR;

namespace ChatsService.Infrastructure.Consumers;

public class UserDeletedConsumer(IMediator mediator) : IConsumer<UserDeletedEvent>
{
    public async Task Consume(ConsumeContext<UserDeletedEvent> context)
    {
        var @event = context.Message;
        var command = new DeleteUserCommand(@event.UserId);
        await mediator.Send(command, context.CancellationToken);
    }
}