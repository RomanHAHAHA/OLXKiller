using Common.Application.Options;
using Common.Infrastructure.Messaging.Events.Product;
using MassTransit;
using Microsoft.Extensions.Options;
using NotificationService.Domain.Interfaces;
using NotificationService.Domain.TransactionCoordinators;

namespace NotificationService.Infrastructure.Consumers;

public class ProductSnapshotUpdateFailedConsumer(
    ITransactionCoordinatorFactory coordinatorFactory,
    ITransactionFailureHandler failureHandler,
    IPublishEndpoint publisher,
    IOptions<ServiceOptions> options) : IConsumer<ProductSnapshotUpdateFailedEvent>
{
    public async Task Consume(ConsumeContext<ProductSnapshotUpdateFailedEvent> context)
    {
        var @event = context.Message;
        var coordinator = coordinatorFactory.GetCoordinator<ProductUpdateCoordinator>();
        
        await failureHandler.TryHandleFailureAsync(
            coordinator, 
            @event.CorrelationId,
            async () =>
            {
                await OnFailure(@event, context.CancellationToken);
            });
    }

    private async Task OnFailure(ProductSnapshotUpdateFailedEvent @event, CancellationToken cancellationToken)
    {
        await publisher.Publish(new ProductRollbackEvent
        {
            CorrelationId = @event.CorrelationId,
            ProductId = @event.ProductId,
            SenderServiceName = options.Value.Name
        }, cancellationToken);
    }
}