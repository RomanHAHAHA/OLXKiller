﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OLXKiller.Domain.Entities;
using OLXKiller.Persistence.Constraints;

namespace OLXKiller.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<ProductEntity>
{
    public void Configure(EntityTypeBuilder<ProductEntity> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(ProductDbConstraints.MAX_NAME_LENGTH);

        builder.Property(p => p.Description)
            .HasMaxLength(ProductDbConstraints.MAX_DESCRIPTION_LENGTH);

        builder.Property(p => p.Price);
        
        builder.Property(p => p.Amount);

        builder.HasMany(p => p.Images)
            .WithOne(i => i.Product)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.UsersWhoLiked)
            .WithMany(u => u.LikedProducts);
    }
}
