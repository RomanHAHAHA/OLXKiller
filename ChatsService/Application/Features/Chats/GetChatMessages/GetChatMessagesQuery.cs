using ChatsService.Application.Features.Messages.Create;
using Common.Domain.Models.Results;
using MediatR;

namespace ChatsService.Application.Features.Chats.GetChatMessages;

public record GetChatMessagesQuery(
    Guid ChatId,
    string? Cursor) : IRequest<CursorPagedList<MessageDto>>;