using Common.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UsersService.Domain.Entities;

namespace UsersService.Infrastructure.Persistence.Configurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermission");
        
        builder.HasKey(x => new { x.RoleId, x.PermissionId });

        builder.HasData(
                Create(Role.Admin, PermissionEnum.ManageActionLogs),
                Create(Role.Admin, PermissionEnum.ManageReviews),
                Create(Role.Admin, PermissionEnum.ManageOrders),
                Create(Role.Admin, PermissionEnum.ManageCategories),
                Create(Role.Admin, PermissionEnum.ManageUsers));
    }

    private static RolePermission Create(Role role, PermissionEnum permission)
    {
        return new RolePermission
        {
            RoleId = role.Id,
            PermissionId = (int)permission
        };
    }
}