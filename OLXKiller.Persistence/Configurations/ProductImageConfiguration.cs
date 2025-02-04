using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OLXKiller.Domain.Entities;

namespace OLXKiller.Persistence.Configurations;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImageEntity>
{
    public void Configure(EntityTypeBuilder<ProductImageEntity> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Data);

        builder.Property(i => i.ProductId);
        builder.HasOne(i => i.Product)
            .WithMany(p => p.Images);
    }
}
