using MediatR;

namespace NotificationService.Application.Features.User.NotifyUserRegistrationFailed;

public record NotifyUserRegistrationFailedCommand(
    Guid CorrelationId,
    string ConnectionId) : IRequest;