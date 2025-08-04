using MediatR;

namespace ProductsService.Application.Features.Users.UpdateAvatar;

public record UpdateUserAvatarCommand(
    Guid UserId,
    string AvatarImageName) : IRequest;