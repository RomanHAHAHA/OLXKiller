using ChatsService.API.Hubs;
using ChatsService.Domain.Interfaces;
using ChatsService.Infrastructure.Persistence;
using Common.Domain.Models.Results;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatsService.Application.Features.Users.SetOnline;

public class SetUserOnlineCommandHandler(
    ChatsDbContext dbContext,
    IHubContext<ChatHub, IChatClient> hubContext) : IRequestHandler<SetUserOnlineCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(SetUserOnlineCommand request, CancellationToken cancellationToken)
    {
        var onlineStatusSet = await dbContext.UserSnapshots
            .Where(u => u.Id == request.UserId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(u => u.LastOnlineAt, DateTime.UtcNow)
                .SetProperty(u => u.IsOnline, true), cancellationToken);

        if (onlineStatusSet == 0)
        {
            return ApiResponse.NotFound("User");
        }

        await hubContext.Clients.All.UserOnline(request.UserId);

        return ApiResponse.Ok();
    }
}