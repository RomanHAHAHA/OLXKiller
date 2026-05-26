using Common.Domain.Entities;
using Common.Infrastructure.Messaging.Events.Product;
using MassTransit;
using MediatR;
using OrdersService.Application.Features.Products.RollbackUpdate;

namespace OrdersService.Infrastructure.Messaging.Consumers;

public class ProductRollbackConsumer(IMediator mediator) : IConsumer<ProductRolledBackEvent>
{
    public async Task Consume(ConsumeContext<ProductRolledBackEvent> context)
    {
        var @event = context.Message;
        var snapshot = new ProductSnapshot
        {
            Id = @event.Id,
            Name = @event.Name,
            Price = @event.Price,
        };
        var command = new ProductUpdateRollbackCommand(snapshot);
        
        await mediator.Send(command, context.CancellationToken);
    }
}