using OLXKiller.Domain.Entities;
using OLXKiller.Domain.Enums;

namespace OLXKiller.Domain.Abstractions.Repositories;

public interface IUsersRepository : IRepository<UserEntity>
{
    Task<UserEntity?> GetByEmailAsync(string email);

    Task<UserEntity?> GetByIdWithAvatar(Guid userId);

    Task<HashSet<Permission>> GetUserPermissions(Guid userId);

    Task<IEnumerable<UserEntity>> GetGroupedUsers();
}
