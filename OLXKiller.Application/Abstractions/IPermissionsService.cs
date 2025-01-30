using OLXKiller.Domain.Enums;

namespace OLXKiller.Application.Abstractions;

public interface IPermissionsService
{
    Task<HashSet<Permission>> GetPermissionsAsync(Guid userId);
}