using Common.Infrastructure.Messaging.Events.User;
using MassTransit;
using MediatR;
using UsersService.Application.Features.Users.RetrievePreviousAvatar;

namespace UsersService.Infrastructure.Messaging.Consumers;

public class UserAvatarUpdateTimeoutConsumer(IServiceProvider serviceProvider) : IConsumer<UserAvatarUpdateTimeoutEvent>
{
    public async Task Consume(ConsumeContext<UserAvatarUpdateTimeoutEvent> context)
    {
        using var scope = serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        var @event = context.Message;
        var command = new RetrievePreviousAvatarCommand(@event.UserId);
        
        await mediator.Send(command, context.CancellationToken);
    }
}