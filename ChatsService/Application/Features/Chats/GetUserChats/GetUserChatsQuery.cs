using MediatR;

namespace ChatsService.Application.Features.Chats.GetUserChats;

public record GetUserChatsQuery(Guid CurrentUserId) : IRequest<List<ChatViewDto>>;