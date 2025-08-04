using Common.Infrastructure.Messaging.Events.Product;
using MassTransit;
using MediatR;
using OrdersService.Application.Features.Products.Update;

namespace OrdersService.Infrastructure.Messaging.Consumers;

public class ProductUpdatedConsumer(IServiceProvider serviceProvider) : IConsumer<ProductUpdatedEvent>
{
    public async Task Consume(ConsumeContext<ProductUpdatedEvent> context)
    {
        using var scope = serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        var @event = context.Message;
        var command = new UpdateProductCommand(
            @event.CorrelationId,
            @event.ProductId,
            @event.UserId,
            @event.Name,
            @event.Price);
        
        await mediator.Send(command, context.CancellationToken);
    }
}