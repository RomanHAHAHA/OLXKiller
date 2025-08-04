using MediatR;
using Microsoft.AspNetCore.SignalR;
using NotificationService.API.Hubs;
using NotificationService.Domain.Interfaces;

namespace NotificationService.Application.Features.Order.NotifyOrderProcessed;

public class NotifyOrderProcessedCommandHandler(
    IHubContext<NotificationHub, INotificationClient> hubContext) : IRequestHandler<NotifyOrderProcessedCommand>
{
    public async Task Handle(NotifyOrderProcessedCommand request, CancellationToken cancellationToken)
    {
        await hubContext.Clients
            .User(request.UserId.ToString())
            .NotifyOrderProcessed("Order processed successfully! Track the status in your order list.");
    }
}