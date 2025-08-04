using CartsService.Application.Features.Products.Delete;
using Common.Infrastructure.Messaging.Events.Product;
using MassTransit;
using MediatR;

namespace CartsService.Infrastructure.Eventing.Consumers;

public class ProductDeletedConsumer(IServiceProvider serviceProvider) : IConsumer<ProductDeletedEvent>
{
    public async Task Consume(ConsumeContext<ProductDeletedEvent> context)
    {
        using var scope = serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        var command = new DeleteProductCommand(context.Message.ProductId);
        await mediator.Send(command, context.CancellationToken);
    }
}