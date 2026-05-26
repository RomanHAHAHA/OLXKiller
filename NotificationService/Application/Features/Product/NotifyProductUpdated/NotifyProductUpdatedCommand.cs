using MediatR;

namespace NotificationService.Application.Features.Product.NotifyProductUpdated;

public record NotifyProductUpdatedCommand(
    Guid CorrelationId,
    string SenderServiceName,
    Guid ProductId,
    Guid UserId) : IRequest;