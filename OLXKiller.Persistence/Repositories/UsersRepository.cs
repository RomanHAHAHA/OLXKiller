using Microsoft.EntityFrameworkCore;
using OLXKiller.Domain.Abstractions.Repositories;
using OLXKiller.Domain.Entities;
using OLXKiller.Domain.Enums;
using OLXKiller.Persistence.Contexts;

namespace OLXKiller.Persistence.Repositories;

public class UsersRepository(AppDbContext appDbContext) :
    Repository<UserEntity>(appDbContext),
    IUsersRepository
{
    public async Task<UserEntity?> GetByEmailAsync(string email)
        => await _appDbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email);

    public async Task<UserEntity?> GetByIdWithAvatar(Guid userId)
        => await _appDbContext.Users
            .Include(u => u.Avatar)
            .FirstOrDefaultAsync(u => u.Id == userId);

    public async Task<HashSet<Permission>> GetUserPermissions(Guid userId)
    {
        var roles = await _appDbContext.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .ThenInclude(r => r.Permissions)
            .Where(u => u.Id == userId)
            .Select(u => u.Role)
            .ToArrayAsync();

        return roles
            .SelectMany(r => r?.Permissions ?? [])
            .Select(p => (Permission)p.Id)
            .ToHashSet();
    }

    public async Task<IEnumerable<UserEntity>> GetGroupedUsers()
    {
        return await GetAllAsync()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(u => u.Role)
            .Include(u => u.Avatar)
            .Select(u => new UserEntity
            {
                Id = u.Id,
                NickName = u.NickName,
                Role = u.Role,
                Avatar = u.Avatar,
            })
            .ToListAsync();
    }
}
