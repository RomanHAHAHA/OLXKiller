using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReviewsService.Domain.Entities;

namespace ReviewsService.Infrastructure.Persistence.Configurations;

public class VotesConfiguration : IEntityTypeConfiguration<ReviewVote>
{
    public void Configure(EntityTypeBuilder<ReviewVote> builder)
    {
        builder.ToTable("Votes");

        builder.HasKey(v => new { v.UserId, v.ReviewUserId, v.ReviewProductId });

        builder.Property(v => v.UserId).IsRequired();
        builder.Property(v => v.ReviewUserId).IsRequired();
        builder.Property(v => v.ReviewProductId).IsRequired();
        builder.Property(v => v.VoteType).IsRequired();

        builder.HasOne(v => v.Review)
            .WithMany(r => r.Votes)
            .HasForeignKey(v => new { v.ReviewUserId, v.ReviewProductId })
            .OnDelete(DeleteBehavior.Cascade); 

        builder.HasIndex(v => new { v.ReviewUserId, v.ReviewProductId });
    }
}