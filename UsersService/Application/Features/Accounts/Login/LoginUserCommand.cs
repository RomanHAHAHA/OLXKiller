using Common.Domain.Models.Results;
using MediatR;

namespace UsersService.Application.Features.Accounts.Login;

public record LoginUserCommand(UserLoginDto UserLoginDto) : IRequest<ApiResponse<string>>;