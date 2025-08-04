using Common.Infrastructure.Messaging.Events.Product;
using MassTransit;
using MediatR;
using NotificationService.Application.Features.Product.NotifyStockExceeded;

namespace NotificationService.Infrastructure.Consumers;

public class ProductStockExceededConsumer(IMediator mediator) : IConsumer<ProductStockExceededEvent>
{
    public async Task Consume(ConsumeContext<ProductStockExceededEvent> context)
    {
        var @event = context.Message;
        var command = new NotifyProductStockExceededCommand(
            @event.UserId,
            @event.StockQuantity);
        await mediator.Send(command, context.CancellationToken);
    }
}