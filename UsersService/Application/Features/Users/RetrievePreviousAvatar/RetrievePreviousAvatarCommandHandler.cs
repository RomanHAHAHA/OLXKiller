using Common.Application.Options;
using Common.Domain.Interfaces;
using Common.Infrastructure.Messaging.Events.User;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Options;
using UsersService.Domain.Entities;
using UsersService.Domain.Interfaces;

namespace UsersService.Application.Features.Users.RetrievePreviousAvatar;

public class RetrievePreviousAvatarCommandHandler(
    IUsersRepository usersRepository,
    ILogger<RetrievePreviousAvatarCommandHandler> logger,
    ICacheService<string> cacheService,
    IPublishEndpoint publisher,
    IOptions<ServiceOptions> serviceOptions) : IRequestHandler<RetrievePreviousAvatarCommand>
{
    public async Task Handle(RetrievePreviousAvatarCommand request, CancellationToken cancellationToken)
    {
        var user = await usersRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            logger.LogInformation("User was not found");
            return;
        }
        
        var previousAvatarPath = await cacheService.GetAsync($"user-avatar:{user.Id}", cancellationToken);

        if (previousAvatarPath is null)
        {
            previousAvatarPath = string.Empty;
        }
        
        user.AvatarPath = previousAvatarPath;
        await OnAvatarRolledBack(user, cancellationToken);
        
        await usersRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task OnAvatarRolledBack(User user, CancellationToken cancellationToken)
    {
        await publisher.Publish(
            new UserAvatarRollbackEvent
            {
                CorrelationId = Guid.NewGuid(),
                SenderServiceName = serviceOptions.Value.Name,
                UserId = user.Id,
                PreviousAvatarName = user.AvatarPath!,
            },
            cancellationToken);
    }
}