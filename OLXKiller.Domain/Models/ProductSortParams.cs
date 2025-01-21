using OLXKiller.Domain.Enums;

namespace OLXKiller.Domain.Models;

public class ProductSortParams
{
    public string? OrderBy { get; set; } = string.Empty;

    public SortDirection? SortDirection { get; set; }
}
