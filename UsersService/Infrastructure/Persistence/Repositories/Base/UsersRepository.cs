using Common.Domain.Abstractions;
using Common.Domain.Dtos;
using Common.Domain.Enums;
using Common.Domain.Extensions;
using Common.Domain.Models.Results;
using Microsoft.EntityFrameworkCore;
using UsersService.Application.Features.Users.GetPagedList;
using UsersService.Domain.Entities;
using UsersService.Domain.Interfaces;

namespace UsersService.Infrastructure.Persistence.Repositories.Base;

public class UsersRepository(UserDbContext dbContext) :
    Repository<UserDbContext, User, Guid>(dbContext),
    IUsersRepository
{
    public async Task<User?> GetByEmailAsync(
        string email, 
        CancellationToken cancellationToken = default)
    {
        return await AppDbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<PagedList<User>> GetPagedList(
        UsersFilter usersFilter,
        SortParams sortParams,
        PageParams pageParams,
        CancellationToken cancellationToken = default)
    {
        return await AppDbContext.Users
            .AsNoTracking()
            .Filter(usersFilter)
            .Sort(sortParams)
            .ToPagedAsync(pageParams, cancellationToken);
    }
    
    public async Task<HashSet<PermissionEnum>> GetPermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var roles = await AppDbContext.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .ThenInclude(r => r!.Permissions)
            .Where(u => u.Id == userId)
            .Select(u => u.Role)
            .ToArrayAsync(cancellationToken);

        return roles
            .SelectMany(r => r?.Permissions ?? [])
            .Select(p => (PermissionEnum)p.Id)
            .ToHashSet();
    }

    public async Task<string?> GetRoleNameAsync(
        User user,
        CancellationToken cancellationToken = default)
    {
        return await AppDbContext.Roles
            .AsNoTracking()
            .Where(r => r.Id == user.RoleId)
            .Select(r => r.Name)
            .FirstOrDefaultAsync(cancellationToken);
    }
}