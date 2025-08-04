using CartsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CartsService.Infrastructure.Persistence.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");
        
        builder.HasKey(x => new { x.UserId, x.ProductId });
        builder.Property(i => i.ProductId).IsRequired();
        builder.Property(i => i.UserId).IsRequired();
        builder.Property(i => i.Quantity).IsRequired();
        
        builder.HasOne(i => i.ProductSnapshot)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .IsRequired();
        
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.UserId, x.ProductId }).IsUnique();
    }
}