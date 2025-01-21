using OLXKiller.Application.Dtos.User;
using OLXKiller.Domain.Abstractions.Models;

namespace OLXKiller.Application.Abstractions;

public interface IUsersService
{
    Task<IBaseResponse> CreateAvatar(Stream imageStream, Guid userId);

    Task<IBaseResponse> UpdateAvatar(Stream imageStream, Guid userId);

    Task<IBaseResponse<LoginedUserViewDto>> GetLoginedUserView(Guid userId);
}