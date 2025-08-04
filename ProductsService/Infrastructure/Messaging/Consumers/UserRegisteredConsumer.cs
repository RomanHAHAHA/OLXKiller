using Common.Infrastructure.Messaging.Events;
using Common.Infrastructure.Messaging.Events.User;
using MassTransit;
using MediatR;
using ProductsService.Application.Features.Users.Create;

namespace ProductsService.Infrastructure.Messaging.Consumers;

public class UserRegisteredConsumer(IMediator mediator) : IConsumer<UserRegisteredEvent>
{
    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var @event = context.Message;
        var command = new CreateUserCommand(@event.UserId, @event.NickName, @event.RegisterDate);
        await mediator.Send(command, context.CancellationToken);
    }
}