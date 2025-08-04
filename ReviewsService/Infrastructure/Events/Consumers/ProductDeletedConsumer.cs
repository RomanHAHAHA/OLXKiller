using Common.Infrastructure.Messaging.Events;
using Common.Infrastructure.Messaging.Events.Product;
using MassTransit;
using MediatR;
using ReviewsService.Application.Features.Products.Delete;

namespace ReviewsService.Infrastructure.Events.Consumers;

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