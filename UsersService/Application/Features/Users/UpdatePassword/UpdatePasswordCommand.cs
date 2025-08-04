using Common.Domain.Models.Results;
using MediatR;

namespace UsersService.Application.Features.Users.UpdatePassword;

public record UpdatePasswordCommand(
    Guid UserId,
    UpdatePasswordDto UpdatePasswordDto) : IRequest<ApiResponse>;