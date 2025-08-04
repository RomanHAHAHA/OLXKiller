using CartsService.Application.Features.CartItems.CleanCart;
using Common.Infrastructure.Messaging.Events.Order;
using MassTransit;
using MediatR;

namespace CartsService.Infrastructure.Eventing.Consumers;

public class OrderProcessedConsumer(IServiceProvider serviceProvider) : IConsumer<OrderProcessedEvent>
{
    public async Task Consume(ConsumeContext<OrderProcessedEvent> context)
    {
        using var scope = serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        var @event = context.Message;
        var command = new CleanCartCommand(@event.UserId);
        await mediator.Send(command, context.CancellationToken);
    }
}