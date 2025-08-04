using ChatsService.API.Hubs;
using ChatsService.Domain.Interfaces;
using ChatsService.Infrastructure.Persistence;
using Common.Domain.Models.Results;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatsService.Application.Features.Messages.Read;

public class ReadMessageCommandHandler(
    ChatsDbContext dbContext,
    IHubContext<ChatHub, IChatClient> hubContext) : IRequestHandler<ReadMessageCommand, ApiResponse<Guid>>
{
    public async Task<ApiResponse<Guid>> Handle(ReadMessageCommand request, CancellationToken cancellationToken)
    {
        var message = await dbContext.Messages
            .AsSingleQuery()
            .Include(m => m.Chat)
            .ThenInclude(c => c!.Users)
            .Include(m => m.Sender)
            .FirstOrDefaultAsync(m => m.Id == request.MessageId, cancellationToken);

        if (message is null)
        {
            return ApiResponse<Guid>.NotFound("Message not found");
        }

        if (message.Chat!.Users.All(u => u.Id != request.CurrentUserId))
        {
            return ApiResponse<Guid>.BadRequest("You are not a participant of this chat");
        }

        if (message.IsRead)
        {
            return ApiResponse<Guid>.Ok(message.Id);
        }

        message.IsRead = true;
        await dbContext.SaveChangesAsync(cancellationToken);

        if (message.SenderId != request.CurrentUserId)
        {
            await hubContext.Clients
                .User(message.SenderId.ToString())
                .MessageRead(message.Id);
        }

        return ApiResponse<Guid>.Ok(message.Id);
    }
}