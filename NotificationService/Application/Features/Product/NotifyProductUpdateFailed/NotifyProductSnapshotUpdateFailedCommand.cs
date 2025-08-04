using MediatR;

namespace NotificationService.Application.Features.Product.NotifyProductUpdateFailed;

public record NotifyProductSnapshotUpdateFailedCommand(
    Guid CorrelationId,
    Guid UserId) : IRequest;