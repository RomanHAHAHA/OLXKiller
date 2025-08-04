using Common.Infrastructure.Messaging.Events.User;
using MassTransit;
using MediatR;
using ReviewsService.Application.Features.Users.RollBackAvatar;

namespace ReviewsService.Infrastructure.Events.Consumers;

public class UserAvatarRollBackConsumer(IServiceProvider serviceProvider) : IConsumer<UserAvatarRollbackEvent>
{
    public async Task Consume(ConsumeContext<UserAvatarRollbackEvent> context)
    {
        using var scope = serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        var @event = context.Message;
        var command = new RollBackUserAvatarCommand(@event.UserId, @event.PreviousAvatarName);
        await mediator.Send(command, context.CancellationToken);
    }
}