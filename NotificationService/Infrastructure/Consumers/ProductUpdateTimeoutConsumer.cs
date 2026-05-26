using Common.Infrastructure.Messaging.Events.Product;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.API.Hubs;
using NotificationService.Domain.Interfaces;
using NotificationService.Domain.TransactionCoordinators;

namespace NotificationService.Infrastructure.Consumers;

public class ProductUpdateTimeoutConsumer(
    IPublishEndpoint publisher,
    ITransactionCoordinatorFactory coordinatorFactory,
    ITransactionTimeoutHandler timeoutHandler,
    IHubContext<NotificationHub, INotificationClient> hubContext) : IConsumer<VerifyProductUpdatedEvent>
{
    public async Task Consume(ConsumeContext<VerifyProductUpdatedEvent> context)
    {
        var @event = context.Message;
        var coordinator = coordinatorFactory.GetCoordinator<ProductUpdateCoordinator>();
        
        await timeoutHandler.TryHandleTimeoutAsync(
            coordinator,
            @event.CorrelationId,
            async () =>
            {
                await OnTimeout(@event, context);
            },
            context.CancellationToken);
    }
    
    private async Task OnTimeout(
        VerifyProductUpdatedEvent @event,
        ConsumeContext<VerifyProductUpdatedEvent> context)
    {
        await hubContext.Clients
            .User(context.Message.UserId.ToString())
            .NotifyProductUpdateFailed("Product update timed out! Try again later.");
        
        await publisher.Publish(new ProductRollbackEvent
        {
            CorrelationId = @event.CorrelationId,
            ProductId = @event.ProductId,
            SenderServiceName = @event.SenderServiceName,
        }, context.CancellationToken);
    }
}