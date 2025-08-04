using Common.Infrastructure.Messaging.Events.Email;
using MassTransit;
using MediatR;
using UsersService.Application.Features.Users.MarkEmailConfirmed;

namespace UsersService.Infrastructure.Messaging.Consumers;

public class EmailConfirmedConsumer(IServiceProvider serviceProvider) : IConsumer<EmailConfirmedEvent>
{
    public async Task Consume(ConsumeContext<EmailConfirmedEvent> context)
    {
        using var scope = serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        var @event = context.Message;
        var command = new MarkEmailAsConfirmedCommand(@event.Email);
        await mediator.Send(command, context.CancellationToken);
    }
}