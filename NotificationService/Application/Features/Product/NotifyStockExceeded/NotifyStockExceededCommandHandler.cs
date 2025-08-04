using MediatR;
using Microsoft.AspNetCore.SignalR;
using NotificationService.API.Hubs;
using NotificationService.Domain.Interfaces;

namespace NotificationService.Application.Features.Product.NotifyStockExceeded;

public class NotifyStockExceededCommandHandler(
    IHubContext<NotificationHub, INotificationClient> hubContext) : 
    IRequestHandler<NotifyProductStockExceededCommand>
{
    public async Task Handle(NotifyProductStockExceededCommand request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"{request.UserId}: Notifying Stock Exceeded {request.StockQuantity}.");
        await hubContext.Clients
            .User(request.UserId.ToString())
            .NotifyProductStockExceeded(request.StockQuantity);
    }
}