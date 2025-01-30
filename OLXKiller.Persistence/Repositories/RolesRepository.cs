using Microsoft.EntityFrameworkCore;
using OLXKiller.Domain.Abstractions.Repositories;
using OLXKiller.Domain.Entities;
using OLXKiller.Persistence.Contexts;

namespace OLXKiller.Persistence.Repositories;

public class RolesRepository(AppDbContext appDbContext) :
    Repository<RoleEntity>(appDbContext),
    IRolesRepository
{
    public async Task<List<RoleEntity>> GetAllRolesAsync() 
        => await _appDbContext.Roles
            .AsNoTracking()
            .ToListAsync();

    public async Task<RoleEntity?> GetByIdAsync(int roleId) 
        => await _appDbContext.Roles.FirstOrDefaultAsync(r => r.Id == roleId);

    public async Task<RoleEntity?> GetByNameAsync(string roleName) 
        => await _appDbContext.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
}
