using CartsService.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CartsService.Infrastructure.Persistence;

public class CartsDbContext(DbContextOptions<CartsDbContext> options) : DbContext(options)
{
    public DbSet<ProductSnapshot> ProductSnapshots { get; set; }

    public DbSet<CartItem> CartItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("cart");
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CartsDbContext).Assembly);
        
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
    }
}