using ChatsService.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ChatsService.Infrastructure.Persistence;

public class ChatsDbContext(DbContextOptions<ChatsDbContext> options) : DbContext(options)
{
    public DbSet<UserSnapshot> UserSnapshots { get; set; }
    
    public DbSet<Chat> Chats { get; set; }
    
    public DbSet<Message> Messages { get; set; }

    public DbSet<UserMute> UserMutes { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("chats");
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ChatsDbContext).Assembly);
        
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
    }
}