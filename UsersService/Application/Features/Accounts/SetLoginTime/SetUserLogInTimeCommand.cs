using MediatR;

namespace UsersService.Application.Features.Accounts.SetLoginTime;

public record SetUserLogInTimeCommand(Guid UserId, DateTime LoggedInTime) : IRequest;