using Common.Domain.Models.Results;
using MediatR;

namespace ChatsService.Application.Features.Chats.Create;

public record CreateChatCommand(
    Guid CurrentUserId,
    Guid OtherUSerId) : IRequest<ApiResponse<Guid>>;