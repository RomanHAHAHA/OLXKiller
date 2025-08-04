using CartsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CartsService.Infrastructure.Persistence.Configurations;

public class ProductSnapshotConfiguration : IEntityTypeConfiguration<ProductSnapshot>
{
    public void Configure(EntityTypeBuilder<ProductSnapshot> builder)
    {
        builder.ToTable("ProductSnapshots");
        
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SellerId);
        builder.Property(x => x.Name).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Price).IsRequired();
        builder.Property(x => x.MainImagePath).IsRequired();
    }
}