using Common.Infrastructure.Messaging.Events.Order;
using EmailService.Application.Features.EmailConfirmations.NotifyOrderStatusChanged;
using MassTransit;
using MediatR;

namespace EmailService.Infrastructure.Eventing.Consumers;

public class OrderStatusChangedConsumer(IMediator mediator) : IConsumer<OrderStatusChangedEvent>
{
    public async Task Consume(ConsumeContext<OrderStatusChangedEvent> context)
    {
        var @event = context.Message;
        var command = new NotifyOrderStatusChangedCommand(
            @event.UserEmail,
            @event.EmailSubject,
            @event.EmailContent);
            
        await mediator.Send(command, context.CancellationToken);
    }
}