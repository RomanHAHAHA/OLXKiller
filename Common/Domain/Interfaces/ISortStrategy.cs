using System.Linq.Expressions;

namespace Common.Domain.Interfaces;

public interface ISortStrategy<T>
{
    Expression<Func<T, object>> GetKeySelector(string? orderBy);
}