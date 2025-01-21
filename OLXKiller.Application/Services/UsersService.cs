using OLXKiller.Application.Abstractions;
using OLXKiller.Application.Dtos.User;
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
    IImageManager _imageManager) : IUsersService
{
    public async Task<IBaseResponse> CreateAvatar(Stream imageStream, Guid userId)
    {
        var userAvatar = await _avatarsRepository.GetByIdAsync(userId);

        if (userAvatar is not null)
        {
            return new BaseResponse(
                HttpStatusCode.Conflict,
                "You already have an avatar!");
        }

        var imageData = await imageStream.ConvertToBytesAsync();
        var avatarEntity = new UserAvatarEntity(userId, imageData);
        await _avatarsRepository.CreateAsync(avatarEntity);

        return new BaseResponse(HttpStatusCode.OK);
    }

    public async Task<IBaseResponse> UpdateAvatar(Stream imageStream, Guid userId)
    {
        var userAvatar = await _avatarsRepository.GetByIdAsync(userId);

        if (userAvatar is null)
        {
            return new BaseResponse(
                HttpStatusCode.NotFound,
                "You dont have an avatar yet!");
        }

        var imageData = await imageStream.ConvertToBytesAsync();
        userAvatar.Data = imageData;
        await _avatarsRepository.UpdateAsync(userAvatar);

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

        var image64String = user.Avatar is null
            ? Convert.ToBase64String(_imageManager.GetDefaultAvatarBytes())
            : Convert.ToBase64String(user.Avatar.Data ?? []);

        var userView = new LoginedUserViewDto
        {
            NickName = user.NickName,
            Avatar64String = image64String
        };

        return new BaseResponse<LoginedUserViewDto>(
            HttpStatusCode.OK,
            data: userView);
    }
}
