using Common.Infrastructure.Messaging.Events.Order;
using MassTransit;
using MediatR;
using ProductsService.Application.Features.Products.Commands.Reserve;

namespace ProductsService.Infrastructure.Messaging.Consumers;

public class OrderCreatedConsumer(IServiceProvider serviceProvider) : IConsumer<OrderCreatedEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        using var scope = serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        var command = new ReserveOrderProductsCommand(
            context.Message.OrderId,
            context.Message.UserId,
            context.Message.CartItems);
        
        await mediator.Send(command, context.CancellationToken);
    }
}