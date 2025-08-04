using Common.Domain.Models.Results;
using MediatR;

namespace UsersService.Application.Features.Accounts.Register;

public record RegisterUserCommand(UserRegisterDto RegisterDto) : IRequest<ApiResponse<Guid>>;