using Common.Domain.Entities;
using LogsService.Domain.Entiites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogsService.Infrastructure.Persistence.Configurations;

public class ActionLogConfiguration : IEntityTypeConfiguration<ActionLog>
{
    public void Configure(EntityTypeBuilder<ActionLog> builder)
    {
        builder.ToTable("ActionLogs");
        
        builder.HasKey(l => l.Id);
        
        builder.Property(l => l.UserId).IsRequired();
        
        builder.Property(l => l.ActionType).IsRequired();
        
        builder.Property(l => l.Description)
            .HasMaxLength(255)
            .IsRequired();
        
        builder.Property(l => l.CreatedAt).IsRequired();
    }
}