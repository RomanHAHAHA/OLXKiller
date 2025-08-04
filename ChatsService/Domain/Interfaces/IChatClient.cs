using ChatsService.Application.Features.Messages.Create;

namespace ChatsService.Domain.Interfaces;

public interface IChatClient
{
    Task ReceiveMessage(MessageDto message);

    Task ReceiveMessageNotification(MessageNotificationDto message);
    
    Task MessageRead(Guid messageId);
    
    Task UserOnline(Guid userId);
    
    Task UserOffline(Guid userId);
}