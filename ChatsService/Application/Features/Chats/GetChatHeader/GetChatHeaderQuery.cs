using Common.Domain.Models.Results;
using MediatR;

namespace ChatsService.Application.Features.Chats.GetChatHeader;

public record GetChatHeaderQuery(
    Guid ChatId, 
    Guid CurrentUserId) : IRequest<ApiResponse<UserChatHeaderDto>>;