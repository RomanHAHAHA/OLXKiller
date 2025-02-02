using Microsoft.Extensions.Options;
using OLXKiller.Application.Abstractions;
using OLXKiller.Application.Dtos.User;
using OLXKiller.Application.Options;
using OLXKiller.Domain.Abstractions.Models;
using OLXKiller.Domain.Abstractions.Providers;
using OLXKiller.Domain.Abstractions.Repositories;
using OLXKiller.Domain.Entities;
using OLXKiller.Domain.Extensions;
using OLXKiller.Domain.Models;
using System.Net;

namespace OLXKiller.Application.Services;

public class UsersService(
    IUsersRepository _usersRepository,
    IRepository<UserAvatarEntity> _avatarsRepository,
    IImageManager _imageManager,
    IOptions<ImageManagerOptions> _imageOptions) : IUsersService
{
    public async Task<IBaseResponse> SetAvatarAsync(Stream imageStream, Guid userId)
    {
        var userAvatar = await _avatarsRepository.GetByIdAsync(userId);
        var imageData = imageStream.ConvertToBytes();

        if (userAvatar is not null)
        {
            userAvatar.Data = imageData;
            await _avatarsRepository.UpdateAsync(userAvatar);

            return new BaseResponse(HttpStatusCode.OK);
        }

        var avatarEntity = new UserAvatarEntity(userId, imageData);
        await _avatarsRepository.CreateAsync(avatarEntity);

        return new BaseResponse(HttpStatusCode.OK);
    }

    public async Task<IBaseResponse<LoginedUserViewDto>> GetLoginedUserView(Guid userId)
    {
        var user = await _usersRepository.GetByIdWithAvatar(userId);

        if (user is null)
        {
            return new BaseResponse<LoginedUserViewDto>(
                HttpStatusCode.NotFound,
                "User with such id not found");
        }

        var imageBytes = user.Avatar is null ? 
            await _imageManager.GetDefaultBytesAsync(_imageOptions.Value.DefaultAvatarImageName) : 
            user.Avatar.Data ?? [];

        var userView = new LoginedUserViewDto(user.NickName, imageBytes);

        return new BaseResponse<LoginedUserViewDto>(
            HttpStatusCode.OK, data: userView);
    }

    public async Task<IEnumerable<CollectionUserDto>> GetGroupedUsers()
    {
        var users = await _usersRepository.GetGroupedUsers();
        var dtos = await Task.WhenAll(users.Select(async u => new CollectionUserDto
        {
            Id = u.Id,
            NickName = u.NickName,
            RoleName = u.Role?.Name ?? "Unknown",
            AvatarBytes = u.Avatar?.Data ?? 
               await _imageManager.GetDefaultBytesAsync(_imageOptions.Value.DefaultAvatarImageName)
        }));

        return dtos;
    }
}
