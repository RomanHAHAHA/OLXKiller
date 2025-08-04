namespace ReviewsService.Domain.Entities;

public class ProductSnapshot
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public decimal Price { get; set; }

    public string MainImagePath { get; set; } = string.Empty;

    public List<Review> Reviews { get; set; } = [];
}