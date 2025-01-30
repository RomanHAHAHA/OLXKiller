using OLXKiller.Application.Abstractions;
using OLXKiller.Domain.Abstractions.Repositories;
using OLXKiller.Domain.Enums;

namespace OLXKiller.Application.Services;

public class PermissionsService(
    IUsersRepository usersRepository) : IPermissionsService
{
    public async Task<HashSet<Permission>> GetPermissionsAsync(Guid userId)
        => await usersRepository.GetUserPermissions(userId);
}
