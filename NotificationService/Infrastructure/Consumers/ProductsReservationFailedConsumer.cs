using Common.Infrastructure.Messaging.Events.Product;
using MassTransit;
using MediatR;
using NotificationService.Application.Features.Product.NotifyProductsReservationFailed;

namespace NotificationService.Infrastructure.Consumers;

public class ProductsReservationFailedConsumer(IMediator mediator) : IConsumer<ProductsReservationFailedEvent>
{
    public async Task Consume(ConsumeContext<ProductsReservationFailedEvent> context)
    {
        var @event = context.Message;
        var command = new NotifyProductsReservationFailedCommand(
            @event.UserId,
            @event.ProductStockInfos);
        await mediator.Send(command, context.CancellationToken);
    }
}