using MediatR;

namespace ReviewsService.Application.Features.Users.RollBackAvatar;

public record RollBackUserAvatarCommand(Guid UserId, string PreviousAvatarName) : IRequest;