using Common.Application.Options;
using Common.Infrastructure.Messaging.Events.User;
using Common.Infrastructure.Messaging.Publishers;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Options;
using ReviewsService.Domain.Entities;
using ReviewsService.Domain.Interfaces;
using ReviewsService.Infrastructure.Persistence;

namespace ReviewsService.Application.Features.Users.Create;

public class CreateUserCommandHandler(
    IUsersRepository usersRepository,
    IPublishEndpoint publisher,
    IOptions<ServiceOptions> serviceOptions) : IRequestHandler<CreateUserCommand>
{
    public async Task Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        await usersRepository.CreateAsync(
            new UserSnapshot
            {
                Id = request.UserId,
                NickName = request.NickName,
            }, 
            cancellationToken);
        
        try
        {
            await OnUserCreated(request, cancellationToken);
        }
        catch (Exception)
        {
            await OnUserCreationFailed(request, cancellationToken);
        }
    }

    private async Task OnUserCreated(CreateUserCommand request, CancellationToken cancellationToken)
    {
        await publisher.Publish(
            new UserSnapshotCreatedEvent
            {
                CorrelationId = request.CorrelationId,
                SenderServiceName = serviceOptions.Value.Name,
                UserId = request.UserId,
                ConnectionId = request.ConnectionId,
            },
            cancellationToken);
        
        await usersRepository.SaveChangesAsync(cancellationToken);
    }
    
    private async Task OnUserCreationFailed(CreateUserCommand request, CancellationToken cancellationToken)
    {
        await publisher.PublishInIsolatedScopeAsync<ReviewsDbContext>(
            new UserSnapshotCreationFailedEvent
            {
                CorrelationId = request.CorrelationId,
                SenderServiceName = serviceOptions.Value.Name,
                UserId = request.UserId,
                ConnectionId = request.ConnectionId,
            },
            cancellationToken);
    }
}