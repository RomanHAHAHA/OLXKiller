using OLXKiller.Domain.Entities;

namespace OLXKiller.Domain.Abstractions.Repositories;

public interface IRolesRepository : IRepository<RoleEntity>
{
    Task<RoleEntity?> GetByIdAsync(int roleId);
}
