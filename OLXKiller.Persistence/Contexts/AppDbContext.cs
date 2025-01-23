using Microsoft.EntityFrameworkCore;
using OLXKiller.Domain.Entities;

namespace OLXKiller.Persistence.Contexts;

public class AppDbContext : DbContext
{
    public DbSet<UserEntity> Users { get; set; }

    public DbSet<UserAvatarEntity> Avatars { get; set; }

    public DbSet<ProductEntity> Products { get; set; }

    public DbSet<ProductImageEntity> ProductImages { get; set; }

    public DbSet<ProductUserLikeEntity> Likes { get; set; }


    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
