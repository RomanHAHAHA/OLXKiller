using Common.Infrastructure.Messaging.Events.Product;
using MassTransit;
using MediatR;
using ProductsService.Application.Features.Products.Commands.Delete;

namespace ProductsService.Infrastructure.Messaging.Consumers;

public class ProductCreationTimeoutConsumer(IMediator mediator) : IConsumer<ProductCreationTimeoutEvent>
{
    public async Task Consume(ConsumeContext<ProductCreationTimeoutEvent> context)
    {
        var @event = context.Message;
        var command = new DeleteProductCommand(@event.UserId, @event.ProductId);
        
        await mediator.Send(command, context.CancellationToken);
    }
}