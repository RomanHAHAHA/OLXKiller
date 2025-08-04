using Common.Domain.Models.Results;
using MediatR;

namespace ChatsService.Application.Features.Users.SetOffline;

public record SetUserOfflineCommand(Guid UserId) : IRequest<ApiResponse>;