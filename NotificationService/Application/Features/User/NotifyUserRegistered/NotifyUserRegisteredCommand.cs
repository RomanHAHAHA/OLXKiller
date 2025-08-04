using MediatR;

namespace NotificationService.Application.Features.User.NotifyUserRegistered;

public record NotifyUserRegisteredCommand(
    Guid CorrelationId,
    string SenderServiceName,
    string ConnectionId) : IRequest;