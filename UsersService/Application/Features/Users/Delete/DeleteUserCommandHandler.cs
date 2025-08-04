using Common.Application.Options;
using Common.Domain.Models.Results;
using Common.Infrastructure.Messaging.Events.User;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Options;
using UsersService.Domain.Entities;
using UsersService.Domain.Interfaces;

namespace UsersService.Application.Features.Users.Delete;

public class DeleteUserCommandHandler(
    IUsersRepository usersRepository,
    IPublishEndpoint publisher,
    IOptions<ServiceOptions> serviceOptions) : IRequestHandler<DeleteUserCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await usersRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return ApiResponse.NotFound(nameof(User));
        }
        
        usersRepository.Delete(user);
        await OnUserDeleted(user.Id, cancellationToken);
        
        var deleted = await usersRepository.SaveChangesAsync(cancellationToken);
        
        return deleted ? ApiResponse.Ok() : ApiResponse.InternalServerError();
    }

    private async Task OnUserDeleted(Guid userId, CancellationToken cancellationToken)
    {
        await publisher.Publish(
            new UserDeletedEvent
            {
                CorrelationId = Guid.NewGuid(),
                SenderServiceName = serviceOptions.Value.Name,
                UserId = userId,
            },
            cancellationToken);
    }
}