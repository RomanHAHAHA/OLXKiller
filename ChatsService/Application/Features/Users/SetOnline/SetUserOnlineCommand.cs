using Common.Domain.Models.Results;
using MediatR;

namespace ChatsService.Application.Features.Users.SetOnline;

public record SetUserOnlineCommand(Guid UserId) : IRequest<ApiResponse>;