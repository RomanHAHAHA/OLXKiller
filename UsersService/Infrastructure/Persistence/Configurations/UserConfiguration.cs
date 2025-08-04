using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UsersService.Domain.Entities;

namespace UsersService.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.NickName)
            .HasMaxLength(255)
            .IsRequired();
        
        builder.Property(u => u.Email)
            .HasMaxLength(255)
            .IsRequired();
        
        builder.HasIndex(u => u.Email).IsUnique();
        
        builder.Property(u => u.PasswordHash).IsRequired();
        
        builder.Property(u => u.AvatarPath);
        
        builder.Property(u => u.RoleId).IsRequired();
        
        builder.HasOne(u => u.Role)
            .WithMany(u => u.Users)
            .HasForeignKey(u => u.RoleId);
        
        builder.Property(u => u.CreatedAt).IsRequired();
        
        builder.Property(u => u.LastLogIn);
    }
}