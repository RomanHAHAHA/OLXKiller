using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrdersService.Domain.Entities;

namespace OrdersService.Infrastructure.Persistence.Configurations;

public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistoryItem>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistoryItem> builder)
    {
        builder.ToTable("OrderStatusHistory");
        
        builder.HasKey(h => h.Id);
        builder.Property(h => h.OrderId).IsRequired();
        builder.Property(h => h.Status).IsRequired();
        builder.Property(h => h.CreatedAt).IsRequired();

        builder.HasOne(h => h.Order)
            .WithMany(o => o.Statuses)
            .HasForeignKey(h => h.OrderId);
        
        builder.HasIndex(h => h.OrderId);
    }
}