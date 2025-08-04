using MediatR;

namespace NotificationService.Application.Features.Product.NotifyProductCreated;

public record NotifyProductSnapshotsCreatedCommand(
    Guid CorrelationId,
    string SenderServiceName,
    Guid ProductId,
    Guid UserId) : IRequest;