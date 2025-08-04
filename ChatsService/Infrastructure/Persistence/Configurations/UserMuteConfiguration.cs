using ChatsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatsService.Infrastructure.Persistence.Configurations;

public class UserMuteConfiguration : IEntityTypeConfiguration<UserMute>
{
    public void Configure(EntityTypeBuilder<UserMute> builder)
    {
        builder.ToTable($"{nameof(UserMute)}s");
        
        builder.HasKey(um => new { um.MutingUserId, um.MutedUserId });

        builder.HasOne(um => um.MutingUser)
            .WithMany(u => u.MutedUsers)
            .HasForeignKey(um => um.MutingUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(um => um.MutedUser)
            .WithMany(u => u.MutedByUsers)
            .HasForeignKey(um => um.MutedUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}