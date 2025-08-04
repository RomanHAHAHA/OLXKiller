using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrdersService.Domain.Entities;

namespace OrdersService.Infrastructure.Persistence.Configurations;

public class DeliveryLocationConfiguration : IEntityTypeConfiguration<DeliveryLocation>
{
    public void Configure(EntityTypeBuilder<DeliveryLocation> builder)
    {
        builder.ToTable("DeliveryLocations");

        builder.HasKey(dl => dl.OrderId);

        builder.Property(dl => dl.Region).IsRequired();
        builder.Property(dl => dl.City).IsRequired();
        builder.Property(dl => dl.Warehouse).IsRequired();
    }
}