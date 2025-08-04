using MediatR;

namespace NotificationService.Application.Features.Order.NotifyOrderProcessed;

public record NotifyOrderProcessedCommand(Guid UserId) : IRequest;