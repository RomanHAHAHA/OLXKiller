using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OLXKiller.Domain.Entities;

namespace OLXKiller.Persistence.Configurations;

public class UserAvatarConfiguration : IEntityTypeConfiguration<UserAvatarEntity>
{
    public void Configure(EntityTypeBuilder<UserAvatarEntity> builder)
    {
        builder.HasKey(a => a.UserId);

        builder.Property(a => a.Data);

        builder.HasOne(a => a.User)
            .WithOne(u => u.Avatar); 
    }
}
