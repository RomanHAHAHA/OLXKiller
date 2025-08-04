using Common.Domain.Abstractions;
using ProductsService.Application.Common.Dtos;
using ProductsService.Application.Features.Products.Commands.Create;

namespace ProductsService.Domain.Entities;

public sealed class Product : Entity<Guid>
{
    public Guid UserId { get; set; }

    public UserSnapshot? User { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public double AverageRating { get; set; }

    public List<Category> Categories { get; set; } = [];

    public List<ProductImage> Images { get; set; } = [];

    public List<ProductCharacteristic> Characteristics { get; set; } = [];
    
    public static Product FromCreateDto(ProductCreateDto createDto, Guid userId)
    {
        return new Product
        {
            UserId = userId,
            Name = createDto.Name,
            Description = createDto.Description,
            Price = createDto.Price!.Value,
            StockQuantity = createDto.StockQuantity!.Value
        };
    }
}