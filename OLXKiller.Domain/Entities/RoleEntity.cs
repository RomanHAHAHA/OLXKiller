using OLXKiller.Domain.Abstractions.Models;

namespace OLXKiller.Domain.Entities;

public sealed class RoleEntity : Enumeration<RoleEntity>
{
    public static readonly RoleEntity User = new(1, nameof(User));

    public static readonly RoleEntity Admin = new(2, nameof(Admin));

    public static readonly RoleEntity Moderator = new(3, nameof(Moderator));

    public RoleEntity(int id, string name) : base(id, name)
    {
    }

    public ICollection<PermissionEntity> Permissions { get; set; } = [];

    public ICollection<UserEntity> Users { get; set; } = [];
}
