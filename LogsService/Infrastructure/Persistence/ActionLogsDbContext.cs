using LogsService.Domain.Entiites;
using LogsService.Infrastructure.Persistence.Configurations;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace LogsService.Infrastructure.Persistence;

public class ActionLogsDbContext(DbContextOptions<ActionLogsDbContext> options) : DbContext(options)
{
    public DbSet<ActionLog> ActionLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("action_logs");
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ActionLogConfiguration).Assembly);
        
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
    }
}