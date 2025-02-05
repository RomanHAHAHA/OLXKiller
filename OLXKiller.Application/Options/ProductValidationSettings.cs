namespace OLXKiller.Application.Options;

public class ProductValidationSettings
{
    public int MaxNameLength { get; set; }

    public int MinNameLength { get; set; }

    public int MaxDescriptionLength { get; set; }

    public int MinDescriptionLength { get; set; }

    public int MaxPriceValue { get; set; }

    public int MinPriceValue { get; set; }

    public int MaxAmountValue { get; set; }

    public int MinAmountValue { get; set; }
}
