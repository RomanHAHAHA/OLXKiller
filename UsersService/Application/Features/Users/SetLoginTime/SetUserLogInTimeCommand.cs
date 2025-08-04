using MediatR;

namespace UsersService.Application.Features.Users.SetLoginTime;

public record SetUserLogInTimeCommand(Guid UserId, DateTime LoggedInTime) : IRequest;