using ChatsService.Domain.Entities;

namespace ChatsService.Application.Features.Messages.Create;

public class MessageDto
{
    public required Guid Id { get; init; } 
    
    public required Guid SenderId { get; init; }
    
    public required string Content { get; init; }
    
    public required bool IsRead { get; init; } 
    
    public required DateTime CreatedAt { get; init; }

    public static MessageDto Create(Message message)
    {
        return new MessageDto
        {
            Id = message.Id,
            SenderId = message.SenderId,
            Content = message.Content,
            IsRead = message.IsRead,
            CreatedAt = message.CreatedAt.ToLocalTime()
        };
    }
}