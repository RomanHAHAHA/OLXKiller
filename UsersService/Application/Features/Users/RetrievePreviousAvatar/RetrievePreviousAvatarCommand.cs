using MediatR;

namespace UsersService.Application.Features.Users.RetrievePreviousAvatar;

public record RetrievePreviousAvatarCommand(Guid UserId) : IRequest;