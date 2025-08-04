using Common.Infrastructure.Messaging.Events.CartItem;
using MassTransit;
using MediatR;
using ProductsService.Application.Features.Products.Commands.CheckStockQuantity;

namespace ProductsService.Infrastructure.Messaging.Consumers;

public class ProductCartQuantityIncrementedConsumer(
    IServiceProvider serviceProvider) : IConsumer<ProductCartQuantityIncrementedEvent>
{
    public async Task Consume(ConsumeContext<ProductCartQuantityIncrementedEvent> context)
    {
        using var scope = serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        var @event = context.Message;
        var command = new CheckProductStockQuantityCommand(
            @event.UserId,
            @event.ProductId, 
            @event.RequestedQuantity);
        
        await mediator.Send(command, context.CancellationToken);
    }
}