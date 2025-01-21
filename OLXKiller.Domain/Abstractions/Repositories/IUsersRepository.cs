using OLXKiller.Domain.Entities;

namespace OLXKiller.Domain.Abstractions.Repositories;

public interface IUsersRepository : IRepository<UserEntity>
{
    Task<UserEntity?> GetByEmail(string email);

    Task<UserEntity?> GetByIdWithAvatar(Guid userId);
}
