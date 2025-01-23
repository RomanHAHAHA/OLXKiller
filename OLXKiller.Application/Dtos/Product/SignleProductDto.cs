namespace OLXKiller.Application.Dtos.Product;

public class SignleProductDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; } = decimal.Zero;

    public int Amount { get; set; }

    public bool IsAvailable => Amount > 0;

    public IEnumerable<string> ImageStrings { get; set; } = []; 
}
