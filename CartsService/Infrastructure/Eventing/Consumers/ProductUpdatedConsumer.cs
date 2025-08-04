using CartsService.Application.Features.Products.Update;
using Common.Infrastructure.Messaging.Events.Product;
using MassTransit;
using MediatR;

namespace CartsService.Infrastructure.Eventing.Consumers;

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