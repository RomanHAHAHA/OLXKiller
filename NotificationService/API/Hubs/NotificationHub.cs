using Microsoft.AspNetCore.SignalR;
using NotificationService.Domain.Interfaces;

namespace NotificationService.API.Hubs;

public class NotificationHub : Hub<INotificationClient>
{
    public string GetConnectionId() => Context.ConnectionId;
}