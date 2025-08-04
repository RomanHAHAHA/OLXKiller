using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductsService.Domain.Entities;

namespace ProductsService.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.UserId);
        
        builder.Property(p => p.Name)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(p => p.Price).IsRequired();
        
        builder.Property(p => p.StockQuantity).IsRequired();
        
        builder.Property(p => p.AverageRating)
            .HasDefaultValue(0.0)
            .IsRequired();
        
        builder.Property(p => p.CreatedAt).IsRequired();

        builder.HasMany(p => p.Categories).WithMany(x => x.Products);
        
        builder.HasMany(p => p.Images)
            .WithOne(p => p.Product)
            .HasForeignKey(p => p.ProductId);
        
        builder.HasOne(p => p.User)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.UserId);
    }
}