using Common.Domain.Models.Results;
using MediatR;

namespace UsersService.Application.Features.Users.Delete;

public record DeleteUserCommand(Guid UserId) : IRequest<ApiResponse>;