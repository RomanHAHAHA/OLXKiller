using Common.Infrastructure.Messaging.Events.Order;
using MassTransit;
using MediatR;
using NotificationService.Application.Features.Order.NotifyOrderProcessed;

namespace NotificationService.Infrastructure.Consumers;

public class OrderProcessedConsumer(IMediator mediator) : IConsumer<OrderProcessedEvent>
{
    public async Task Consume(ConsumeContext<OrderProcessedEvent> context)
    {
        var @event = context.Message;
        var command = new NotifyOrderProcessedCommand(@event.UserId);
        await mediator.Send(command, context.CancellationToken); 
    }
}