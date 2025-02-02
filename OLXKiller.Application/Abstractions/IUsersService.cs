using OLXKiller.Application.Dtos.User;
using OLXKiller.Domain.Abstractions.Models;

namespace OLXKiller.Application.Abstractions;

public interface IUsersService
{
    Task<IBaseResponse> SetAvatarAsync(Stream imageStream, Guid userId);

    Task<IBaseResponse<LoginedUserViewDto>> GetLoginedUserView(Guid userId);

    Task<IEnumerable<CollectionUserDto>> GetGroupedUsers();
}