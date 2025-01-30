using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using OLXKiller.Domain.Entities;
using OLXKiller.Domain.Enums;

namespace OLXKiller.Persistence.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<PermissionEntity>
{
    public void Configure(EntityTypeBuilder<PermissionEntity> builder)
    {
        builder.ToTable("Permissions");

        builder.HasKey(p => p.Id);

        var permissions = Enum.GetValues<Permission>()
            .Select(p => new PermissionEntity
            {
                Id = (int)p,
                Name = p.ToString()
            });

        builder.HasData(permissions);
    }
}
