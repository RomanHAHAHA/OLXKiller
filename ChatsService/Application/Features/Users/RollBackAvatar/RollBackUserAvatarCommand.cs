using MediatR;

namespace ChatsService.Application.Features.Users.RollBackAvatar;

public record RollBackUserAvatarCommand(Guid UserId, string PreviousAvatarName) : IRequest;