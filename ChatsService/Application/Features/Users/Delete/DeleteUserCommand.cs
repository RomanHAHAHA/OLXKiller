using MediatR;

namespace ChatsService.Application.Features.Users.Delete;

public record DeleteUserCommand(Guid UserId) : IRequest;