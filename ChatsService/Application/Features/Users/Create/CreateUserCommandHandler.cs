using ChatsService.Domain.Entities;
using ChatsService.Infrastructure.Persistence;
using Common.Application.Options;
using Common.Infrastructure.Messaging.Events.User;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Options;

namespace ChatsService.Application.Features.Users.Create;

public class CreateUserCommandHandler(
    ChatsDbContext dbContext,
    IPublishEndpoint publisher,
    IOptions<ServiceOptions> serviceOptions) : IRequestHandler<CreateUserCommand>
{
    public async Task Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = new UserSnapshot()
        {
            Id = request.UserId,
            NickName = request.NickName,
        };

        try
        {
            await dbContext.UserSnapshots.AddAsync(user, cancellationToken);
            await OnUserCreated(request, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

        }
        catch (Exception)
        {
            await OnUserCreationFailed(request, cancellationToken);    
            await dbContext.SaveChangesAsync(cancellationToken);
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
    }
    
    private async Task OnUserCreationFailed(CreateUserCommand request, CancellationToken cancellationToken)
    {
        await publisher.Publish(
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