namespace OLXKiller.Domain.Entities;

public class ProductEntity 
{
    public const int MAX_NAME_LENGTH = 200;  
    public const int MIN_NAME_LENGTH = 5;  
    public const int MAX_DESCRIPTION_LENGTH = 1000;  
    public const int MIN_DESCRIPTION_LENGTH = 20;  
    public const int MAX_PRICE_VALUE = 1000000;  
    public const int MIN_PRICE_VALUE = 0;  
    public const int MAX_AMOUNT_VALUE = 1000000;  
    public const int MIN_AMOUNT_VALUE = 0;  

    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Amount { get; set; }

    public Guid SellerId { get; set; }

    public UserEntity? Seller { get; set; }

    public ICollection<ProductImageEntity> Images { get; set; } = [];
}
