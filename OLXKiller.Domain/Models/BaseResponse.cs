using OLXKiller.Domain.Abstractions.Models;
using System.Net;

namespace OLXKiller.Domain.Models;

public class BaseResponse<T> : IBaseResponse<T>
{
    public string Description { get; set; } = string.Empty;

    public HttpStatusCode Status { get; set; }

    public T Data { get; set; }

    public BaseResponse(
        HttpStatusCode statusCode = HttpStatusCode.OK,
        string error = "",
        T data = default)
    {
        Status = statusCode;
        Description = error;
        Data = data;
    }
}

public class BaseResponse : IBaseResponse
{
    public string Description { get; set; } = string.Empty;

    public HttpStatusCode Status { get; set; }

    public BaseResponse(
        HttpStatusCode statusCode = HttpStatusCode.OK,
        string error = "")
    {
        Status = statusCode;
        Description = error;
    }
}
