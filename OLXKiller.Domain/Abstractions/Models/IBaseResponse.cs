using System.Net;

namespace OLXKiller.Domain.Abstractions.Models;

public interface IBaseResponse<T>
{
    T Data { get; set; }

    string Description { get; set; }

    HttpStatusCode Status { get; set; }

    public bool IsSuccess => Status == HttpStatusCode.OK;

    public bool IsFailure => !IsSuccess;
}

public interface IBaseResponse
{
    string Description { get; set; }

    HttpStatusCode Status { get; set; }

    public bool IsSuccess => Status == HttpStatusCode.OK;

    public bool IsFailure => !IsSuccess;
}
