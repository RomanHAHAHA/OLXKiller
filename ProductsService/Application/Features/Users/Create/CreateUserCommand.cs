using MediatR;

namespace ProductsService.Application.Features.Users.Create;

public record CreateUserCommand(Guid UserId, string NickName, DateTime RegisterDate) : IRequest; 