using ChatsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatsService.Infrastructure.Persistence.Configurations;

public class UserSnapshotConfiguration : IEntityTypeConfiguration<UserSnapshot>
{
    public void Configure(EntityTypeBuilder<UserSnapshot> builder)
    {
        builder.ToTable(nameof(UserSnapshot) + "s");
        
        builder.HasKey(u => u.Id);
        builder.Property(u => u.NickName).HasMaxLength(255).IsRequired();
        builder.Property(u => u.AvatarImageName).IsRequired();
        builder.Property(u => u.IsOnline).HasDefaultValue(false);
        builder.Property(u => u.LastOnlineAt).HasDefaultValue(null);

        builder.HasMany(u => u.Chats).WithMany(c => c.Users);
    }
}