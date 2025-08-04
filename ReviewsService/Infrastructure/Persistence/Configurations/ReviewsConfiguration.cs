using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReviewsService.Domain.Entities;

namespace ReviewsService.Infrastructure.Persistence.Configurations;

public class ReviewsConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");
        
        builder.HasKey(r => new { r.UserId, r.ProductId });
        
        builder.Property(r => r.UserId).IsRequired();
        builder.Property(r => r.ProductId).IsRequired();
        builder.Property(r => r.Text).HasMaxLength(255).IsRequired();
        builder.Property(r => r.Rate).IsRequired();
        builder.Property(r => r.Status).IsRequired();
        builder.Property(r => r.CreatedAt).IsRequired();

        builder.HasOne(r => r.Product)
            .WithMany(p => p.Reviews)
            .HasForeignKey(r => r.ProductId);
        
        builder.HasOne(r => r.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.UserId);
        
        builder.HasMany(r => r.Votes)
            .WithOne(v => v.Review)
            .HasForeignKey(v => new { v.ReviewUserId, v.ReviewProductId });
        
        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.ProductId);
    }
}