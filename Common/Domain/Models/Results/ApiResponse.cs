using System.Net;
using FluentValidation.Results;

namespace Common.Domain.Models.Results;

public class ApiResponse<T>(
    HttpStatusCode statusCode = HttpStatusCode.OK,
    object? error = null,
    T data = default)
{
    public object? Error { get; set; } = error;

    public HttpStatusCode Status { get; set; } = statusCode;

    public T Data { get; set; } = data;

    public bool IsSuccess => Status == HttpStatusCode.OK;

    public bool IsFailure => !IsSuccess;

    public static ApiResponse<T> NotFound(string notFoundEntityName) 
        => new(HttpStatusCode.NotFound, $"{notFoundEntityName} was not found");

    public static ApiResponse<T> Ok(T data) => new(data: data);

    public static ApiResponse<T> InternalServerError(string description = "Unexpected error occured during the request") 
        => new(HttpStatusCode.InternalServerError, description);

    public static ApiResponse<T> Conflict(string description = "")
        => new(HttpStatusCode.Conflict, description);
    
    public static ApiResponse<T> UnAuthorized(string description = "")
        => new(HttpStatusCode.Unauthorized, description);
    
    public static ApiResponse<T> BadRequest(ValidationResult validationResult)
        => new(HttpStatusCode.BadRequest, validationResult);

    public static ApiResponse<T> BadRequest(string description = "")
        => new(HttpStatusCode.BadRequest, description);
    
    public static implicit operator ApiResponse<T>(T value) => Ok(value);
}

public class ApiResponse
{
    public object? Description { get; set; } 

    public HttpStatusCode Status { get; set; }

    public bool IsSuccess => Status == HttpStatusCode.OK;

    public bool IsFailure => !IsSuccess;
    
    private ApiResponse(
        HttpStatusCode statusCode = HttpStatusCode.OK,
        object? description = null)
    {
        Status = statusCode;
        Description = description;
    }

    public static ApiResponse NotFound(string notFoundEntityName)
    {
        return new ApiResponse(
            HttpStatusCode.NotFound,
            $"{notFoundEntityName} was not found");
    }

    public static ApiResponse Ok(string message = "") => new();

    public static ApiResponse InternalServerError(string error = "Unexpected error occured during the request") 
        => new(HttpStatusCode.InternalServerError, error);

    public static ApiResponse Conflict(string error = "") 
        => new(HttpStatusCode.Conflict, error);

    public static ApiResponse BadRequest(string error = "")
        => new(HttpStatusCode.BadRequest, error);
    
    public static ApiResponse BadRequest(ValidationResult validationResult)
        => new(HttpStatusCode.BadRequest, validationResult);
}