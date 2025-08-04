using Common.Application.Options;
using Common.Infrastructure.Messaging.Events.User;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Options;
using OrdersService.Domain.Interfaces;

namespace OrdersService.Application.Features.Users.UpdateAvatar;

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
            await OnAvatarUpdateFailure(request, cancellationToken);
            await usersRepository.SaveChangesAsync(cancellationToken);
            return;
        }

        try
        {
            user.AvatarPath = request.AvatarPath;
            await OnAvatarUpdated(request, cancellationToken);

            await usersRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            await OnAvatarUpdateFailure(request, cancellationToken);
            await usersRepository.SaveChangesAsync(cancellationToken);
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
    }
    
    private async Task OnAvatarUpdateFailure(UpdateUserAvatarCommand request, CancellationToken cancellationToken)
    {
        await publisher.Publish(
            new UserSnapshotAvatarUpdateFailureEvent
            {
                CorrelationId = request.CorrelationId,
                SenderServiceName = serviceOptions.Value.Name,
                UserId = request.UserId,
            },
            cancellationToken);
    }
}