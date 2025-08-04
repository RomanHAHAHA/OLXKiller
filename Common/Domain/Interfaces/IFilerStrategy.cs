namespace Common.Domain.Interfaces;

public interface IFilterStrategy<T, TFilter>
{
    IQueryable<T> Filter(IQueryable<T> query, TFilter filter);
}