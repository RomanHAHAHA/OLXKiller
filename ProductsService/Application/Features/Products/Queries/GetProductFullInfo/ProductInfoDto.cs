using ProductsService.Domain.Dtos;

namespace ProductsService.Application.Features.Products.Queries.GetProductFullInfo;

public class ProductInfoDto
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public decimal Price { get; set; }
    
    public int StockQuantity { get; set; }

    public double Rating { get; set; }
    
    public bool IsMine { get; set; } 
    
    public ProductSellerDto Seller { get; set; } = null!;

    public List<ShortImageDto> Images { get; set; } = [];

    public List<ShortCategoryDto> Categories { get; set; } = [];

    public List<ProductCharacteristicViewDto> Characteristics { get; set; } = [];
}