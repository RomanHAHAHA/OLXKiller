using ChatsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatsService.Infrastructure.Persistence.Configurations;

public class ChatConfiguration : IEntityTypeConfiguration<Chat>
{
    public void Configure(EntityTypeBuilder<Chat> builder)
    {
        builder.ToTable(nameof(Chat) + "s");
        
        builder.HasKey(c => c.Id);
        builder.Property(c => c.CreatedAt).IsRequired();
        
        builder.HasMany(c => c.Messages)
            .WithOne(m => m.Chat)
            .HasForeignKey(m => m.ChatId);
        
        builder.HasMany(c => c.Users)
            .WithMany(p => p.Chats);
    }
}