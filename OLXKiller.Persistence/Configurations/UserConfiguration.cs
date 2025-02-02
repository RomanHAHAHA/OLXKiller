using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OLXKiller.Domain.Entities;

namespace OLXKiller.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.NickName)
            .HasMaxLength(UserEntity.MAX_NICK_NAME_LENGTH);

        builder.Property(u => u.Email)
            .HasMaxLength(UserEntity.MAX_EMAIL_LENGTH);

        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.HashedPassword);

        builder.HasOne(u => u.Avatar)
            .WithOne(a => a.User);

        builder.HasMany(u => u.LikedProducts)
            .WithMany(p => p.UsersWhoLiked);

        builder.Property(u => u.RoleId).HasDefaultValue(1);

        builder.HasOne(u => u.Role)
            .WithMany(r => r.Users);
    }
}
