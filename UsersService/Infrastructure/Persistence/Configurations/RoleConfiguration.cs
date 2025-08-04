using Common.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UsersService.Domain.Entities;

namespace UsersService.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);

        builder.HasMany(r => r.Users).WithOne(u => u.Role);

        builder.HasMany(r => r.Permissions)
            .WithMany()
            .UsingEntity<RolePermission>(
                j => j.HasOne(pr => pr.Permission)
                    .WithMany()
                    .HasForeignKey(pr => pr.PermissionId),
                j => j.HasOne(pr => pr.Role)
                    .WithMany()
                    .HasForeignKey(pr => pr.RoleId)
            );

        builder.HasData(Role.GetValues());
    }
}