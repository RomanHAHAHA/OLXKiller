using MediatR;

namespace ChatsService.Application.Features.Users.Create;

public record CreateUserCommand(
    Guid CorrelationId,
    Guid UserId,
    string NickName,
    string ConnectionId) : IRequest; 