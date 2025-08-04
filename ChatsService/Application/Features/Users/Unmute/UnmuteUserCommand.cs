using Common.Domain.Models.Results;
using MediatR;

namespace ChatsService.Application.Features.Users.Unmute;

public record UnmuteUserCommand(Guid CurrentUserId, Guid UserToUnmuteId) : IRequest<ApiResponse>;