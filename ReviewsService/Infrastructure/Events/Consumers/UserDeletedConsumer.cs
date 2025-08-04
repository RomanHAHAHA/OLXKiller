using Common.Infrastructure.Messaging.Events.User;
using MassTransit;
using MediatR;
using ReviewsService.Application.Features.Users.Delete;

namespace ReviewsService.Infrastructure.Events.Consumers;

public class UserDeletedConsumer(IServiceProvider serviceProvider) : IConsumer<UserDeletedEvent>
{
    public async Task Consume(ConsumeContext<UserDeletedEvent> context)
    {
        using var scope = serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        var @event = context.Message;
        var command = new DeleteUserCommand(@event.UserId);
        await mediator.Send(command, context.CancellationToken);
    }
}