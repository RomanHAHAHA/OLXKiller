using ChatsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatsService.Infrastructure.Persistence.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable(nameof(Message) + "s");
        
        builder.HasKey(m => m.Id);
        
        builder.Property(m => m.Content).IsRequired().HasMaxLength(2000);
        builder.Property(m => m.CreatedAt).IsRequired();
        builder.Property(m => m.IsRead).IsRequired().HasDefaultValue(false);
        
        builder.HasOne(m => m.Chat)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ChatId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(m => m.Sender);
        
        builder.HasIndex(m => m.ChatId); 
        builder.HasIndex(m => m.SenderId);
    }
}