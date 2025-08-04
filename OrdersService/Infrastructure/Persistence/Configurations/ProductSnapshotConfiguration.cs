using Common.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OrdersService.Infrastructure.Persistence.Configurations;

public class ProductSnapshotConfiguration : IEntityTypeConfiguration<ProductSnapshot>
{
    public void Configure(EntityTypeBuilder<ProductSnapshot> builder)
    {
        builder.ToTable("ProductSnapshots");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(p => p.Name).HasMaxLength(255).IsRequired();
        builder.Property(p => p.Price).IsRequired();
        builder.Property(p => p.MainImagePath);
    }
}