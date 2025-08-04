using Common.Infrastructure.Messaging.Events.Product;
using MassTransit;
using MediatR;
using OrdersService.Application.Features.Products.SetMainImage;

namespace OrdersService.Infrastructure.Messaging.Consumers;

public class ProductMainImageSetConsumer(IServiceProvider serviceProvider) : IConsumer<ProductMainImageSetEvent> 
{
    public async Task Consume(ConsumeContext<ProductMainImageSetEvent> context)
    {
        using var scope = serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        var @event = context.Message;
        var command = new SetMainProductImageCommand(
            @event.CorrelationId,
            @event.ProductId,
            @event.ImagePath);
        
        await mediator.Send(command, context.CancellationToken);
    }
}