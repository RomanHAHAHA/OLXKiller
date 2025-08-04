using Common.Infrastructure.Messaging.Events.User;
using MassTransit;
using MediatR;
using OrdersService.Application.Features.Users.Create;

namespace OrdersService.Infrastructure.Messaging.Consumers;

public class UserRegisteredConsumer(IServiceProvider serviceProvider) : IConsumer<UserRegisteredEvent>
{
    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        using var scope = serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        var @event = context.Message;
        var command = new CreateUserCommand(
            @event.CorrelationId,
            @event.UserId, 
            @event.NickName, 
            @event.Email,
            @event.ConnectionId);
        
        await mediator.Send(command, context.CancellationToken);
    }
}