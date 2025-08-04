using Common.Infrastructure.Messaging.Events.User;
using MassTransit;
using MediatR;
using ProductsService.Application.Features.Users.UpdateAvatar;

namespace ProductsService.Infrastructure.Messaging.Consumers;

public class UserAvatarUpdatedConsumer(IServiceProvider serviceProvider) : IConsumer<UserAvatarUpdatedEvent>
{
    public async Task Consume(ConsumeContext<UserAvatarUpdatedEvent> context)
    {
        using var scope = serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        var @event = context.Message;
        var command = new UpdateUserAvatarCommand(@event.UserId, @event.AvatarPath);
        await mediator.Send(command, context.CancellationToken);
    }
}