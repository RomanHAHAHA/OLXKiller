using Common.Domain.Models.Results;
using MediatR;

namespace UsersService.Application.Features.Accounts.UpdatePassword;

public record UpdatePasswordCommand(
    Guid UserId,
    UpdatePasswordDto UpdatePasswordDto) : IRequest<ApiResponse>;