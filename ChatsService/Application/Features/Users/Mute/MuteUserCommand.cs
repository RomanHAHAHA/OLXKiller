using Common.Domain.Models.Results;
using MediatR;

namespace ChatsService.Application.Features.Users.Mute;

public record MuteUserCommand(Guid CurrentUserId, Guid UserToMuteId) : IRequest<ApiResponse>;