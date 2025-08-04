using MediatR;

namespace NotificationService.Application.Features.User.NotifyAvatarUpdated;

public record NotifyAvatarUpdatedCommand(
    Guid CorrelationId,
    string SenderServiceName,
    Guid UserId) : IRequest;