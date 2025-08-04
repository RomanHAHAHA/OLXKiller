using MediatR;
using ProductsService.Domain.Interfaces;

namespace ProductsService.Application.Features.Users.UpdateAvatar;

public class UpdateUserAvatarCommandHandler(
    IUsersRepository usersRepository,
    ILogger<UpdateUserAvatarCommand> logger) : IRequestHandler<UpdateUserAvatarCommand>
{
    public async Task Handle(UpdateUserAvatarCommand request, CancellationToken cancellationToken)
    {
        var user = await usersRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            logger.LogWarning($"User with id {request.UserId} not found");
            return;
        }
        
        user.AvatarImageName = request.AvatarImageName;
        var updated = await usersRepository.SaveChangesAsync(cancellationToken);
        
        var message = updated ? 
            $"User with id {request.UserId} was updated" :
            $"Failed to update user with id {request.UserId}";
        
        logger.LogInformation(message);
    }
}