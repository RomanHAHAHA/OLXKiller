using MediatR;
using Microsoft.AspNetCore.SignalR;
using NotificationService.API.Hubs;
using NotificationService.Domain.Interfaces;

namespace NotificationService.Application.Features.User.NotifyUserRegistrationFailed;

public class NotifyUserRegistrationFailedCommandHandler(
    IHubContext<NotificationHub, INotificationClient> hubContext,
    IRedisLockService redisLockService) : IRequestHandler<NotifyUserRegistrationFailedCommand>
{
    public async Task Handle(NotifyUserRegistrationFailedCommand request, CancellationToken cancellationToken)
    {
        await hubContext.Clients
            .Client(request.ConnectionId)
            .NotifyUserRegistrationFailed();
        
        var lockKey = $"lock:product-created:{request.CorrelationId}";
        await redisLockService.ReleaseLockAsync(lockKey);
    }
}