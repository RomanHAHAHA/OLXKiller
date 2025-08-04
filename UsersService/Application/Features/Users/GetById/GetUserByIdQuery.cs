using Common.Domain.Entities;
using Common.Domain.Models.Results;
using MediatR;
using UsersService.Domain.Entities;

namespace UsersService.Application.Features.Users.GetById;

public record GetUserByIdQuery(Guid UserId) : IRequest<ApiResponse<User>>;
