using MediatR;

namespace ReviewsService.Application.Features.Users.Delete;

public record DeleteUserCommand(Guid UserId) : IRequest;