using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductsService.Domain.Entities;

namespace ProductsService.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<UserSnapshot>
{
    public void Configure(EntityTypeBuilder<UserSnapshot> builder)
    {
        builder.ToTable("UserSnapshots");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.NickName).IsRequired();
        builder.Property(x => x.AvatarImageName).IsRequired();
        builder.Property(x => x.Rating).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasMany(u => u.Products)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId);
    }
}