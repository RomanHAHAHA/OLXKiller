using Common.Application.Options;
using Common.Domain.Interfaces;
using Common.Domain.Models.Results;
using Common.Infrastructure.Messaging.Events.User;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Options;
using UsersService.Domain.Entities;
using UsersService.Domain.Interfaces;

namespace UsersService.Application.Features.Users.SetAvatarImage;

public class SetAvatarImageCommandHandler(
    IUsersRepository usersRepository,
    IFileStorageService fileStorageService,
    IPublishEndpoint publishEndpoint,
    IOptions<UserImagesOptions> userImagesOptions,
    IOptions<ServiceOptions> serviceOptions,
    ICacheService<string> cacheService) : IRequestHandler<SetAvatarImageCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(SetAvatarImageCommand request, CancellationToken cancellationToken)
    {
        var user = await usersRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return ApiResponse.NotFound(nameof(User));
        }

        var result = await fileStorageService.SaveFileAsync(
            request.Image.File,
            userImagesOptions.Value.Path,
            cancellationToken);

        if (result.IsFailure)
        {
            return ApiResponse.InternalServerError(result.Error);
        }
        
        var oldAvatarPath = user.AvatarPath;
        user.AvatarPath = result.Value;
        await OnAvatarSet(user, cancellationToken);

        var updated = await usersRepository.SaveChangesAsync(cancellationToken);

        if (!updated)
        {
            return ApiResponse.InternalServerError();
        }
        
        await cacheService.SetAsync(
            $"user-avatar:{user.Id}",
            oldAvatarPath ?? string.Empty,
            TimeSpan.FromMinutes(5),
            cancellationToken);

        return ApiResponse.Ok();
    }
    
    private async Task OnAvatarSet(User user, CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(
            new UserAvatarUpdatedEvent
            {
                CorrelationId = Guid.NewGuid(),
                SenderServiceName = serviceOptions.Value.Name,
                UserId = user.Id,
                AvatarPath = user.AvatarPath!,
            }, 
            cancellationToken);
    }
}