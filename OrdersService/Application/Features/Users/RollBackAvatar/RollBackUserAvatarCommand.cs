using MediatR;

namespace OrdersService.Application.Features.Users.RollBackAvatar;

public record RollBackUserAvatarCommand(Guid UserId, string PreviousAvatarName) : IRequest;