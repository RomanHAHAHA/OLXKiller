using Common.Domain.Models.Results;
using MediatR;

namespace ChatsService.Application.Features.Chats.ExistsWithUser;

public record CheckChatWithUserExistsQuery(
    Guid CurrentUserId,
    Guid OtherUserId) : IRequest<ApiResponse<Guid>>;