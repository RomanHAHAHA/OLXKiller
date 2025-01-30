using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using OLXKiller.Domain.Entities;

namespace OLXKiller.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<RoleEntity>
{
    public void Configure(EntityTypeBuilder<RoleEntity> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);

        builder.HasMany(r => r.Users)
            .WithMany(u => u.Roles);

        builder.HasMany(r => r.Permissions)
            .WithMany()
            .UsingEntity<RolePermissionEntity>(
                j => j.HasOne(pr => pr.Permission)
                    .WithMany()
                    .HasForeignKey(pr => pr.PermissionId),
                j => j.HasOne(pr => pr.Role)
                    .WithMany()
                    .HasForeignKey(pr => pr.RoleId)
            );

        builder.HasData(RoleEntity.GetValues());
    }
}
