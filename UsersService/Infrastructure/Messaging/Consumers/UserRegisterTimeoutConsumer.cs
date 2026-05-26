using Common.Infrastructure.Messaging.Events.User;
using MassTransit;
using MediatR;
using UsersService.Application.Features.Users.Delete;

namespace UsersService.Infrastructure.Messaging.Consumers;

public class UserRegisterTimeoutConsumer(IServiceProvider serviceProvider) : IConsumer<UserRegisterTimeoutEvent>
{
    public async Task Consume(ConsumeContext<UserRegisterTimeoutEvent> context)
    {
        using var scope = serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        var @event = context.Message;
        var command = new DeleteUserCommand(@event.UserId);
        await mediator.Send(command, context.CancellationToken);
    }
}