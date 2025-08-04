using System.Security.Claims;
using ChatsService.Application.Features.Users.SetOffline;
using ChatsService.Application.Features.Users.SetOnline;
using ChatsService.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace ChatsService.API.Hubs;

public class ChatHub(
    IChatConnectionTracker connectionTracker,
    IMediator mediator) : Hub<IChatClient>
{
    public override async Task OnConnectedAsync()
    {
        var userId = GetCurrentUserId();
        await connectionTracker.SetConnectionAsync(userId, Context.ConnectionId, Guid.Empty);
        await mediator.Send(new SetUserOnlineCommand(userId));
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetCurrentUserId();
        await connectionTracker.FullRemoveConnectionAsync(Context.ConnectionId);
        await mediator.Send(new SetUserOfflineCommand(userId));
        
        await base.OnDisconnectedAsync(exception);
    }
    
    public async Task JoinChatGroup(Guid chatId)
    {
        var userId = GetCurrentUserId();
        await connectionTracker.SetConnectionAsync(userId, Context.ConnectionId, chatId);
        await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
    }

    public async Task LeaveChatGroup(Guid chatId)
    {
        var userId = GetCurrentUserId();
        await connectionTracker.SetConnectionAsync(userId, Context.ConnectionId, Guid.Empty);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId.ToString());
    }
    
    public string GetConnectionId() => Context.ConnectionId;
    
    private Guid GetCurrentUserId()
    {
        var claim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || !Guid.TryParse(claim.Value, out var userId))
        {
            throw new HubException("User not authenticated");
        }
        return userId;
    }
}