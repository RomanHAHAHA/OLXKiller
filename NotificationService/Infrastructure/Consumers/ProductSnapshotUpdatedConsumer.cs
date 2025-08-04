using Common.Infrastructure.Messaging.Events.Product;
using MassTransit;
using MediatR;
using NotificationService.Application.Features.Product.NotifyProductUpdated;

namespace NotificationService.Infrastructure.Consumers;

public class ProductSnapshotUpdatedConsumer(IMediator mediator) : IConsumer<ProductSnapshotUpdatedEvent>
{
    public async Task Consume(ConsumeContext<ProductSnapshotUpdatedEvent> context)
    {
        var @event = context.Message;
        var command = new NotifyProductSnapshotUpdatedCommand(
            @event.CorrelationId,
            @event.SenderServiceName,
            @event.ProductId,
            @event.UserId);
        
        await mediator.Send(command, context.CancellationToken);
    }
}