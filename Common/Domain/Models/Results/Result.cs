namespace Common.Domain.Models.Results;

public class Result<T>
{
    public readonly T Value;

    public readonly string Error = string.Empty;

    public Result(T value) => Value = value;

    public Result(string error) => Error = error;

    public bool HasValue => string.IsNullOrWhiteSpace(Error);

    public bool IsFailure => !HasValue;

    public static Result<T> Success(T value) => new(value);

    public static Result<T> Failure(string error = "") => new(error);

    public static implicit operator Result<T>(T value) => Success(value);
}

public class Result
{
    public readonly string Error;

    public Result() => Error = string.Empty;

    public Result(string error) => Error = error;

    public bool HasValue => string.IsNullOrEmpty(Error);

    public bool IsFailure => !HasValue;

    public static Result Success() => new();

    public static Result Failure(string error) => new(error);
}