using ChatsService.API.Hubs;
using ChatsService.Domain.Entities;
using ChatsService.Domain.Interfaces;
using ChatsService.Infrastructure.Persistence;
using Common.Domain.Models.Results;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatsService.Application.Features.Messages.Create;

public class CreateMessageCommandHandler(
    ChatsDbContext dbContext,
    IHubContext<ChatHub, IChatClient> hubContext,
    IChatConnectionTracker connectionTracker) : IRequestHandler<CreateMessageCommand, ApiResponse<Guid>>
{
    public async Task<ApiResponse<Guid>> Handle(CreateMessageCommand request, CancellationToken cancellationToken)
    {
        var chat = await dbContext.Chats
            .Include(c => c.Users)
            .FirstOrDefaultAsync(c => c.Id == request.ChatId, cancellationToken);

        if (chat is null)
        {
            return ApiResponse<Guid>.NotFound("Chat not found");
        }

        var sender = chat.Users.FirstOrDefault(u => u.Id == request.CurrentUserId);
        var recipient = chat.Users.FirstOrDefault(u => u.Id != request.CurrentUserId);
        
        if (sender is null || recipient is null)
        {
            return ApiResponse<Guid>.BadRequest("Invalid chat participants");
        }

        var message = new Message
        {
            ChatId = request.ChatId,
            SenderId = request.CurrentUserId,
            Content = request.Content
        };

        await dbContext.Messages.AddAsync(message, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await hubContext.Clients
            .Group(request.ChatId.ToString())
            .ReceiveMessage(MessageDto.Create(message));
        
        var isRecipientInChat = await connectionTracker.IsUserInChatAsync(recipient.Id, request.ChatId);
        var isSenderMuted = await IsCurrentUserMuted(sender.Id, recipient.Id, cancellationToken);

        if (!isRecipientInChat && !isSenderMuted)
        {
            await hubContext.Clients
                .User(recipient.Id.ToString())
                .ReceiveMessageNotification(MessageNotificationDto.Create(message, sender));   
        }

        return ApiResponse<Guid>.Ok(message.Id);
    }

    private async Task<bool> IsCurrentUserMuted(
        Guid senderId, 
        Guid recipientId,
        CancellationToken cancellationToken)
    {
        return await dbContext.UserMutes
            .AnyAsync(um =>
                um.MutingUserId == recipientId &&
                um.MutedUserId == senderId, cancellationToken);
    }
}