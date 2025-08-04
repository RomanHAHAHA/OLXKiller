namespace ChatsService.Application.Features.Chats.GetChatHeader;

public class UserChatHeaderDto
{
    public Guid Id { get; init; }

    public string NickName { get; init; } = string.Empty;

    public string AvatarImageName { get; init; } = string.Empty;

    public bool IsOnline { get; init; }
    
    public DateTime? LastOnlineAt { get; init; }
}