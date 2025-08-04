namespace Common.Domain.Dtos;

public class DataWrapper<T>
{
    public T Data { get; set; } = default!;
}