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

    public byte[] GetDefaultAvatarBytes()
    {
        var defaultAvatarPath = Path.Combine(
            _options.ImagesDirectory,
            _options.DefaultAvatarImageName);

        if (File.Exists(defaultAvatarPath))
        {
            return File.ReadAllBytes(defaultAvatarPath);
        }

        throw new FileNotFoundException($"Default avatar image not found at {defaultAvatarPath}");
    }
}
