using Common.Infrastructure.Messaging.Events.Product;
using MassTransit;
using MediatR;
using NotificationService.Application.Features.Product.NotifyProductCreated;

namespace NotificationService.Infrastructure.Consumers;

public class ProductSnapshotCreatedConsumer(IMediator mediator) :  IConsumer<ProductSnapshotCreatedEvent>
{
    public async Task Consume(ConsumeContext<ProductSnapshotCreatedEvent> context)
    {
        var @event = context.Message;
        var command = new NotifyProductSnapshotsCreatedCommand(
            @event.CorrelationId,
            @event.SenderServiceName,
            @event.ProductId,
            @event.UserId);
        
        await mediator.Send(command, context.CancellationToken);
    }
}