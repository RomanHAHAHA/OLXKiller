using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReviewsService.Domain.Entities;

namespace ReviewsService.Infrastructure.Persistence.Configurations;

public class UsersConfiguration : IEntityTypeConfiguration<UserSnapshot>
{
    public void Configure(EntityTypeBuilder<UserSnapshot> builder)
    {
        builder.ToTable("UserSnapshots");
        
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NickName).IsRequired();
        builder.Property(x => x.AvatarPath).IsRequired();
        
        builder.HasMany(u => u.Reviews)
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId);
    }
}