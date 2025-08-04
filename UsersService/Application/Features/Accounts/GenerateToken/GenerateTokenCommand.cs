using Common.Domain.Models.Results;
using MediatR;

namespace UsersService.Application.Features.Accounts.GenerateToken;

public record GenerateTokenCommand(Guid UserId) : IRequest<ApiResponse<string>>;