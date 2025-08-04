using MassTransit;
using Microsoft.EntityFrameworkCore;
using ReviewsService.Domain.Entities;
using ProductSnapshot = ReviewsService.Domain.Entities.ProductSnapshot;

namespace ReviewsService.Infrastructure.Persistence;

public class ReviewsDbContext(DbContextOptions<ReviewsDbContext> options) : DbContext(options)
{
    public DbSet<Review> Reviews { get; set; }
    
    public DbSet<UserSnapshot> UserSnapshots { get; set; }
    
    public DbSet<ProductSnapshot> ProductSnapshots { get; set; }
    
    public DbSet<ReviewVote> ReviewsVotes { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("reviews");
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReviewsDbContext).Assembly);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}