namespace Common.Domain.Interfaces;

public interface IHttpUserContext
{
    Guid UserId { get; }
    
    string AccessToken { get; }
}