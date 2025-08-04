using Common.Domain.Models.Results;
using MediatR;

namespace UsersService.Application.Features.Users.SetAvatarImage;

public record SetAvatarImageCommand(Guid UserId, SetAvatarImageDto Image) : IRequest<ApiResponse>;
