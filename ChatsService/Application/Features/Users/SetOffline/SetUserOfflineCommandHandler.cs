using ChatsService.API.Hubs;
using ChatsService.Domain.Interfaces;
using ChatsService.Infrastructure.Persistence;
using Common.Domain.Models.Results;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatsService.Application.Features.Users.SetOffline;

public class SetUserOfflineCommandHandler(
    ChatsDbContext dbContext,
    IHubContext<ChatHub, IChatClient> hubContext) : IRequestHandler<SetUserOfflineCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(SetUserOfflineCommand request, CancellationToken cancellationToken)
    {
        var onlineStatusSet = await dbContext.UserSnapshots
            .Where(u => u.Id == request.UserId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(u => u.LastOnlineAt, DateTime.UtcNow)
                .SetProperty(u => u.IsOnline, false), cancellationToken);

        if (onlineStatusSet == 0)
        {
            return ApiResponse.NotFound("User");
        }

        await hubContext.Clients.All.UserOffline(request.UserId);
        
        return ApiResponse.Ok();
    }
}