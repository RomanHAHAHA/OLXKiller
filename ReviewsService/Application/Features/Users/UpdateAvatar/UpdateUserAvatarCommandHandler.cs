using Common.Application.Options;
using Common.Infrastructure.Messaging.Events.User;
using Common.Infrastructure.Messaging.Publishers;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Options;
using ReviewsService.Domain.Interfaces;
using ReviewsService.Infrastructure.Persistence;

namespace ReviewsService.Application.Features.Users.UpdateAvatar;

public class UpdateUserAvatarCommandHandler(
    IUsersRepository usersRepository,
    IPublishEndpoint publisher,
    IOptions<ServiceOptions> serviceOptions) : IRequestHandler<UpdateUserAvatarCommand>
{
    public async Task Handle(UpdateUserAvatarCommand request, CancellationToken cancellationToken)
    {
        var user = await usersRepository.GetByIdAsync(request.UserId, cancellationToken);
            
        if (user is null)
        {
            await OnAvatarUpdateFailed(request, cancellationToken);
            return;
        }

        user.AvatarPath = request.AvatarPath;
        
        try
        {
            await OnAvatarUpdated(request, cancellationToken);
        }
        catch
        {
            await OnAvatarUpdateFailed(request, cancellationToken);
        }
    }

    private async Task OnAvatarUpdated(UpdateUserAvatarCommand request, CancellationToken cancellationToken)
    {
        await publisher.Publish(
            new UserSnapshotAvatarUpdatedEvent
            {
                CorrelationId = request.CorrelationId,
                SenderServiceName = serviceOptions.Value.Name,
                UserId = request.UserId,
            },
            cancellationToken);
        
        await usersRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task OnAvatarUpdateFailed(UpdateUserAvatarCommand request, CancellationToken cancellationToken)
    {
        await publisher.PublishInIsolatedScopeAsync<ReviewsDbContext>(
            new UserSnapshotAvatarUpdateFailureEvent
            {
                CorrelationId = request.CorrelationId,
                SenderServiceName = serviceOptions.Value.Name,
                UserId = request.UserId,
            },
            cancellationToken);
    }
}