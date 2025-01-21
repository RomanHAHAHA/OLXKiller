namespace OLXKiller.Domain.Models;

public class ProductFilter
{
    public string? Name { get; set; } = string.Empty;

    public decimal? MinPrice { get; set; } = decimal.Zero;

    public bool IsAvailable { get; set; } 
}
