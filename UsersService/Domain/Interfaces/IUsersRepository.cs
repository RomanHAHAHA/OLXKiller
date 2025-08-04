using Common.Domain.Dtos;
using Common.Domain.Enums;
using Common.Domain.Interfaces;
using Common.Domain.Models.Results;
using UsersService.Application.Features.Users.GetPagedList;
using UsersService.Domain.Entities;

namespace UsersService.Domain.Interfaces;

public interface IUsersRepository : IRepository<User, Guid>
{
    Task<User?> GetByEmailAsync(
        string email, 
        CancellationToken cancellationToken = default);

    Task<PagedList<User>> GetPagedList(
        UsersFilter usersFilter,
        SortParams sortParams,
        PageParams pageParams,
        CancellationToken cancellationToken = default);
    
    Task<HashSet<PermissionEnum>> GetPermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<string?> GetRoleNameAsync(
        User user,
        CancellationToken cancellationToken = default);
}