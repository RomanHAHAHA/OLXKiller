using ChatsService.Domain.Entities;

namespace ChatsService.Application.Features.Messages.Create;

public class MessageNotificationDto
{
    public required Guid ChatId { get; init; }

    public required string Content { get; init; }

    public required string SenderNickName { get; init; }
    
    public required string SenderAvatarPath { get; init; }

    public static MessageNotificationDto Create(Message message, UserSnapshot sender)
    {
        const int maxPreviewLength = 20;
        var content = message.Content;
    
        var previewContent = content.Length > maxPreviewLength 
            ? content[..maxPreviewLength] + "..." 
            : content;

        return new MessageNotificationDto
        {
            ChatId = message.ChatId,
            Content = previewContent, 
            SenderNickName = sender.NickName,
            SenderAvatarPath = sender.AvatarImageName
        };
    }
}