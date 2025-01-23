using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using OLXKiller.Domain.Entities;

namespace OLXKiller.Persistence.Configurations;

public class ProductUserLikeConfiguration : IEntityTypeConfiguration<ProductUserLikeEntity>
{
    public void Configure(EntityTypeBuilder<ProductUserLikeEntity> builder)
    {
        builder.HasKey(pul => new { pul.ProductId, pul.UserId }); 

        builder.HasOne(pul => pul.Product)
            .WithMany(p => p.UsersWhoLiked)
            .HasForeignKey(pul => pul.ProductId)
            .OnDelete(DeleteBehavior.Restrict); 

        builder.HasOne(pul => pul.User)
            .WithMany(u => u.LikedProducts)
            .HasForeignKey(pul => pul.UserId)
            .OnDelete(DeleteBehavior.Restrict); 

        builder.ToTable("ProductLikes");
    }
}
