using Common.Application.Options;
using Common.Infrastructure.Messaging.Events.Product;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using NotificationService.API.Hubs;
using NotificationService.Domain.Interfaces;
using NotificationService.Domain.TransactionCoordinators;

namespace NotificationService.Infrastructure.Consumers;

public class VerifyProductCreatedConsumer(
    IPublishEndpoint publisher,
    ITransactionCoordinatorFactory coordinatorFactory,
    ITransactionTimeoutHandler timeoutHandler,
    IOptions<ServiceOptions> options,
    IHubContext<NotificationHub, INotificationClient> hubContext) : IConsumer<VerifyProductCreatedEvent>
{
    public async Task Consume(ConsumeContext<VerifyProductCreatedEvent> context)
    {
        var @event = context.Message;
        var coordinator = coordinatorFactory.GetCoordinator<ProductCreationCoordinator>();
        
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
        VerifyProductCreatedEvent @event,
        ConsumeContext<VerifyProductCreatedEvent> context)
    {
        await hubContext.Clients
            .User(context.Message.UserId.ToString())
            .NotifyProductCreationFailed("Product creation timed out! Try again later.");
        
        await publisher.Publish(new ProductCreationTimeoutEvent
        {
            CorrelationId = @event.CorrelationId,
            UserId = @event.UserId,
            ProductId = @event.ProductId,
            SenderServiceName = options.Value.Name,
        }, context.CancellationToken);
    }
}