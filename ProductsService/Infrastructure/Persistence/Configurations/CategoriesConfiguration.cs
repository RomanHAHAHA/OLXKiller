using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductsService.Domain.Entities;

namespace ProductsService.Infrastructure.Persistence.Configurations;

public class CategoriesConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Name)
            .HasMaxLength(255)
            .IsRequired();
        
        builder.HasIndex(c => c.Name).IsUnique();
        
        builder.Property(c => c.Description)
            .HasMaxLength(1000)
            .IsRequired();
        
        builder.HasMany(c => c.Products).WithMany(x => x.Categories);
    }
}