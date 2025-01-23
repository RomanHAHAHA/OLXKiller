using Microsoft.Extensions.Options;
using OLXKiller.Application.Options;
using OLXKiller.Domain.Abstractions.Providers;

namespace OLXKiller.Infrastructure.Services;

public class ImageManager : IImageManager
{
    private readonly ImageManagerOptions _options;

    public ImageManager(IOptions<ImageManagerOptions> options)
    {
        _options = options.Value;
    }

    public async Task<byte[]> GetDefaultBytesAsync(string imageName)
    {
        if (string.IsNullOrWhiteSpace(imageName))
        {
            return [];
        }

        var defaultAvatarPath = Path.Combine(_options.ImagesDirectory, imageName);

        if (File.Exists(defaultAvatarPath))
        {
            return await File.ReadAllBytesAsync(defaultAvatarPath);
        }

        return [];
    }
}
