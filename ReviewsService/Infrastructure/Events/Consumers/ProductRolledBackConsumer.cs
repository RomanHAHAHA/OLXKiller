using Common.Infrastructure.Messaging.Events.Product;
using MassTransit;
using MediatR;
using ReviewsService.Application.Features.Products.RollbackUpdate;
using ReviewsService.Domain.Entities;

namespace ReviewsService.Infrastructure.Events.Consumers;

public class ProductRolledBackConsumer(IMediator mediator) : IConsumer<ProductRolledBackEvent>
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