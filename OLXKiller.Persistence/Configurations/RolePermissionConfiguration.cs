using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using OLXKiller.Domain.Entities;
using OLXKiller.Domain.Enums;

namespace OLXKiller.Persistence.Configurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermissionEntity>
{
    public void Configure(EntityTypeBuilder<RolePermissionEntity> builder)
    {
        builder.HasKey(x => new { x.RoleId, x.PermissionId });

        builder.HasData(
            Create(RoleEntity.Admin, Permission.DeleteAllProducts),
            Create(RoleEntity.Admin, Permission.RemoveOtherUserAvatar),
            Create(RoleEntity.Admin, Permission.AssignRoleToUser)
            );
    }

    private static RolePermissionEntity Create(RoleEntity role, Permission permission)
    {
        return new RolePermissionEntity
        {
            RoleId = role.Id,
            PermissionId = (int)permission
        };
    }
}
