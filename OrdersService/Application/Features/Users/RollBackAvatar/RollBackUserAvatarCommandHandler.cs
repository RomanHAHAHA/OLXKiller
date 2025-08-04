using MediatR;
using OrdersService.Domain.Interfaces;

namespace OrdersService.Application.Features.Users.RollBackAvatar;

public class RollBackUserAvatarCommandHandler(
    IUsersRepository usersRepository,
    ILogger<RollBackUserAvatarCommandHandler> logger) : IRequestHandler<RollBackUserAvatarCommand>
{
    public async Task Handle(RollBackUserAvatarCommand request, CancellationToken cancellationToken)
    {
        var user = await usersRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            logger.LogInformation($"User with id: {request.UserId} not found");
            return;
        }
        
        user.AvatarPath = request.PreviousAvatarName;
        await usersRepository.SaveChangesAsync(cancellationToken);
    }
}