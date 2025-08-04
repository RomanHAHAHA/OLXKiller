using ChatsService.Domain.Entities;

namespace ChatsService.Application.Features.Chats.GetUserChats;

public class ChatViewDto
{
    public required Guid Id { get; init; }
    
    public required UserSnapshot OtherUser { get; init; }
    
    public required int UnreadMessages { get; init; }

    public required bool IsMuted { get; init; }
    
    public required DateTime LastMessageSentAt { get; init; }
}
