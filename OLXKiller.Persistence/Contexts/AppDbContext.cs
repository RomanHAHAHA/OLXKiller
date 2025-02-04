using Microsoft.EntityFrameworkCore;
using OLXKiller.Domain.Entities;

namespace OLXKiller.Persistence.Contexts;

public class AppDbContext : DbContext
{
    public DbSet<UserEntity> Users { get; set; }

    public DbSet<UserAvatarEntity> Avatars { get; set; }

    public DbSet<ProductEntity> Products { get; set; }

    public DbSet<ProductImageEntity> ProductImages { get; set; }

    public DbSet<PermissionEntity> Permissions { get; set; }

    public DbSet<RoleEntity> Roles { get; set; }

    public DbSet<RolePermissionEntity> RolePermissions { get; set; }

    public DbSet<CartItemEntity> CartItems { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
