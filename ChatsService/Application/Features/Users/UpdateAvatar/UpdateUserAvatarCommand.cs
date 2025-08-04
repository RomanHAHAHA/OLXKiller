using MediatR;

namespace ChatsService.Application.Features.Users.UpdateAvatar;

public record UpdateUserAvatarCommand(
    Guid CorrelationId,
    Guid UserId,
    string AvatarPath) : IRequest;