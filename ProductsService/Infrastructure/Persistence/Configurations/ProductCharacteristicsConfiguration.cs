using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductsService.Domain.Entities;

namespace ProductsService.Infrastructure.Persistence.Configurations;

public class ProductCharacteristicsConfiguration : IEntityTypeConfiguration<ProductCharacteristic>
{
    public void Configure(EntityTypeBuilder<ProductCharacteristic> builder)
    {
        builder.ToTable("ProductCharacteristics");
        
        builder.HasKey(pc => pc.Id); 
        builder.HasIndex(pc => new { pc.ProductId, pc.Name }).IsUnique();

        builder.Property(pc => pc.ProductId).IsRequired();
        builder.Property(pc => pc.Name).HasMaxLength(255).IsRequired();
        builder.Property(pc => pc.Value).HasMaxLength(255).IsRequired();
    }
}