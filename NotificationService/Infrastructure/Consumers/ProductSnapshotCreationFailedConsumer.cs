using Common.Infrastructure.Messaging.Events.Product;
using MassTransit;
using MediatR;
using NotificationService.Application.Features.Product.NotifyProductCreationFailed;

namespace NotificationService.Infrastructure.Consumers;

public class ProductSnapshotCreationFailedConsumer(
    IMediator mediator) : IConsumer<ProductSnapshotCreationFailedEvent>
{
    public async Task Consume(ConsumeContext<ProductSnapshotCreationFailedEvent> context)
    {
        Console.WriteLine("Snapshot creation failed");
        var @event = context.Message;
        var command = new NotifyProductSnapshotCreationFailedCommand(
            @event.CorrelationId,
            @event.UserId);
        
        await mediator.Send(command, context.CancellationToken);
    }
}