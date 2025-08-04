using Common.Infrastructure.Messaging.Events.Product;
using MassTransit;
using MediatR;
using OrdersService.Application.Features.Orders.Delete;

namespace OrdersService.Infrastructure.Messaging.Consumers;

public class ProductsReservationFailedConsumer(IMediator mediator) : IConsumer<ProductsReservationFailedEvent>
{
    public async Task Consume(ConsumeContext<ProductsReservationFailedEvent> context)
    {
        var @event = context.Message;
        var command = new DeleteOrderCommand(@event.OrderId);
        await mediator.Send(command, context.CancellationToken);
    }
}