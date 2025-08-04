using Common.Infrastructure.Messaging.Events.SystemAction;
using LogsService.Application.Features.ActionLogs.LogSystemAction;
using MassTransit;
using MediatR;

namespace LogsService.Infrastructure.Messaging.Consumers;

public class SystemActionConsumer(IServiceProvider serviceProvider) : IConsumer<SystemActionEvent>
{
    public async Task Consume(ConsumeContext<SystemActionEvent> context)
    {
        using var scope = serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        var @event = context.Message;
        var command = new CreateLogActionCommand(
            @event.UserId,
            @event.ActionType,
            @event.Message);
        
        await mediator.Send(command, context.CancellationToken);
    }
}