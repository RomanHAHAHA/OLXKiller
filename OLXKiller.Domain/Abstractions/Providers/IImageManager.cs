namespace OLXKiller.Domain.Abstractions.Providers;

public interface IImageManager
{
    Task<byte[]> GetDefaultBytesAsync(string imageName);
}