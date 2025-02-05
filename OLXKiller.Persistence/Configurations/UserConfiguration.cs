using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OLXKiller.Domain.Entities;
using OLXKiller.Persistence.Constraints;

namespace OLXKiller.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.NickName)
            .HasMaxLength(UserDbConstraints.MAX_NICK_NAME_LENGTH);

        builder.Property(u => u.Email)
            .HasMaxLength(UserDbConstraints.MAX_EMAIL_LENGTH);

        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.HashedPassword)
            .HasMaxLength(UserDbConstraints.MAX_PASSWORD_LENGTH);

        builder.HasOne(u => u.Avatar)
            .WithOne(a => a.User);

        builder.HasMany(u => u.LikedProducts)
            .WithMany(p => p.UsersWhoLiked);

        builder.Property(u => u.RoleId).HasDefaultValue(1);
        builder.HasOne(u => u.Role)
            .WithMany(r => r.Users);

        builder.HasMany(u => u.CartItems)
            .WithOne(ci => ci.User);
    }
}
