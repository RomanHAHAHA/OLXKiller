namespace Common.Domain.Models.Results;

public class CursorPagedList<T>
{
    public List<T> Items { get; init; } = [];

    public string? Cursor { get; init; } = string.Empty;

    public bool HasMore { get; init; }   
    
    public static CursorPagedList<T> Empty() => new()
    {
        Items = [],
        Cursor = null,
        HasMore = false
    };
}