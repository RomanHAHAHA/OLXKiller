namespace OLXKiller.Application.Dtos.Product;

public class SingleProductDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; } = decimal.Zero;

    public int Amount { get; set; }

    public bool IsAvailable => Amount > 0;

    public bool Liked { get; set; }

    public IEnumerable<string> ImageStrings { get; set; } = [];

    public Guid SellerId { get; set; }

    public string SellerNickName { get; set; } = string.Empty;

    public string SellerAvatar { get; set; } = string.Empty;

    //review, rating
}
