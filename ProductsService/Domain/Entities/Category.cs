using Common.Domain.Abstractions;
using ProductsService.Application.Common.Dtos;

namespace ProductsService.Domain.Entities;

public class Category : Entity<Guid>
{
    public string Name { get; set; } = string.Empty;

    public Guid? ParentCategoryId { get; set; }

    public Category? ParentCategory { get; set; } 
    
    public List<Category> SubCategories { get; set; } = [];
    
    public List<Product> Products { get; set; } = [];
    
    public static Category FromCreateDto(CategoryCreateDto categoryCreateDto)
    {
        return new Category
        {
            Name = categoryCreateDto.Name,
            ParentCategoryId = categoryCreateDto.ParentCategoryId
        };
    }
}