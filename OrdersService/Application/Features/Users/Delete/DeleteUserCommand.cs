using MediatR;

namespace OrdersService.Application.Features.Users.Delete;

public record DeleteUserCommand(Guid UserId) : IRequest;