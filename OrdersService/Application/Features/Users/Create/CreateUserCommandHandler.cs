using Common.Application.Options;
using Common.Infrastructure.Messaging.Events.User;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Options;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Interfaces;

namespace OrdersService.Application.Features.Users.Create;

public class CreateUserCommandHandler(
    IUsersRepository usersRepository,
    IPublishEndpoint publisher,
    IOptions<ServiceOptions> serviceOptions) : IRequestHandler<CreateUserCommand>
{
    public async Task Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = new UserSnapshot
        {
            Id = request.UserId,
            NickName = request.NickName,
            Email = request.Email,
        };

        try
        {
            await usersRepository.CreateAsync(user, cancellationToken);
            await OnUserCreated(request, cancellationToken);
            await usersRepository.SaveChangesAsync(cancellationToken);

        }
        catch (Exception)
        {
            await OnUserCreationFailed(request, cancellationToken);    
            await usersRepository.SaveChangesAsync(cancellationToken);
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