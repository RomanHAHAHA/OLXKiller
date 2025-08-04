using Common.Infrastructure.Messaging.Events.Product;
using MassTransit;
using MediatR;
using OrdersService.Application.Features.Orders.Confirm;

namespace OrdersService.Infrastructure.Messaging.Consumers;

public class ProductsReservedConsumer(IServiceProvider serviceProvider) : IConsumer<ProductsReservedEvent>
{
    public async Task Consume(ConsumeContext<ProductsReservedEvent> context)
    {
        using var scope = serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        var command = new ConfirmOrderProcessingCommand(
            context.Message.OrderId, 
            context.Message.UserId);
        
        await mediator.Send(command, context.CancellationToken);
    }
}