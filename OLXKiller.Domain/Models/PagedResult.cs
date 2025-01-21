namespace OLXKiller.Domain.Models;

public record PagedResult<T>(IEnumerable<T> Collection, int TotalCount)
{
}
