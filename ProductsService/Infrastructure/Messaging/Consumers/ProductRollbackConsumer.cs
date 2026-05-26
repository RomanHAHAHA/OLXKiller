using Common.Infrastructure.Messaging.Events.Product;
using MassTransit;
using MediatR;
using ProductsService.Application.Features.Products.Commands.RollBackUpdate;

namespace ProductsService.Infrastructure.Messaging.Consumers;

public class ProductRollbackConsumer(IMediator mediator) : IConsumer<ProductRollbackEvent> 
{
    public async Task Consume(ConsumeContext<ProductRollbackEvent> context)
    {
        var @event = context.Message;
        var command = new RollbackProductUpdateCommand(@event.ProductId, @event.CorrelationId);
        
        await mediator.Send(command, context.CancellationToken);
    }
}