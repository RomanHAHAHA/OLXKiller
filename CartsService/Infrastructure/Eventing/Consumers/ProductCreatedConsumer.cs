using CartsService.Application.Features.Products.Create;
using Common.Infrastructure.Messaging.Events.Product;
using MassTransit;
using MediatR;

namespace CartsService.Infrastructure.Eventing.Consumers;

public class ProductCreatedConsumer(IServiceProvider serviceProvider) : IConsumer<ProductCreatedEvent>
{
    public async Task Consume(ConsumeContext<ProductCreatedEvent> context)
    {
        using var scope = serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        var @event = context.Message;
        var command = new CreateProductCommand(
            @event.CorrelationId,
            @event.ProductId,
            @event.SellerId,
            @event.Name,
            @event.Price);

        await mediator.Send(command, context.CancellationToken);
    }
}