using Common.Infrastructure.Messaging.Events.User;
using MassTransit;
using MediatR;
using UsersService.Application.Features.Users.SetLoginTime;

namespace UsersService.Infrastructure.Messaging.Consumers;

public class UserLoggedInConsumer(IMediator mediator) : IConsumer<UserLoggedInEvent>
{
    public async Task Consume(ConsumeContext<UserLoggedInEvent> context)
    {
        var @event = context.Message;
        var command = new SetUserLogInTimeCommand(@event.UserId, @event.LogInTime);
        await mediator.Send(command, context.CancellationToken);
    }
}