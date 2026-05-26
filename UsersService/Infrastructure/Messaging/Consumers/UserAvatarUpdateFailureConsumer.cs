using Common.Infrastructure.Messaging.Events.User;
using MassTransit;
using MediatR;
using UsersService.Application.Features.Users.RetrievePreviousAvatar;

namespace UsersService.Infrastructure.Messaging.Consumers;

public class UserAvatarUpdateFailureConsumer(IServiceProvider serviceProvider) : IConsumer<UserAvatarUpdateFailureEvent>
{
    public async Task Consume(ConsumeContext<UserAvatarUpdateFailureEvent> context)
    {
        using var scope = serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        var @event = context.Message;
        var command = new RetrievePreviousAvatarCommand(@event.UserId);
        await mediator.Send(command, context.CancellationToken);
    }
}