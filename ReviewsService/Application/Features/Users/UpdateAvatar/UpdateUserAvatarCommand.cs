using MediatR;

namespace ReviewsService.Application.Features.Users.UpdateAvatar;

public record UpdateUserAvatarCommand(
    Guid CorrelationId,
    Guid UserId, 
    string AvatarPath) : IRequest;