using MediatR;

namespace NotificationService.Application.Features.User.NotifyAvatarUpdateFailure;

public record NotifyAvatarUpdateFailedCommand(Guid CorrelationId, Guid UserId) : IRequest;