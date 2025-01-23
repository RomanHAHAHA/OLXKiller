using Microsoft.Extensions.Options;
using OLXKiller.Application.Abstractions;
using OLXKiller.Application.Dtos.Product;
using OLXKiller.Application.Options;
using OLXKiller.Domain.Abstractions.Providers;
using OLXKiller.Domain.Entities;

namespace OLXKiller.Application.Factories;

public class ProductDtoFactory(
    IImageManager _imageManager, 
    IOptions<ImageManagerOptions> _imageOptions) : IProductDtoFactory
{
    public async Task<CollectionProductDto> CreateCollectionDtoAsync(
        ProductEntity productEntity,
        Guid currentUserId)
    {
        var imageData = productEntity.Images.Any()
            ? productEntity.Images.First().Data
            : await _imageManager.GetDefaultBytesAsync(_imageOptions.Value.DefaultProductImageName);

        var liked = false;

        if (!currentUserId.Equals(Guid.Empty))
        {
            liked = productEntity.UsersWhoLiked
                .FirstOrDefault(like => like.UserId == currentUserId) is not null;
        }

        return new CollectionProductDto
        {
            Id = productEntity.Id,
            Name = productEntity.Name,
            Description = productEntity.Description,
            Price = productEntity.Price,
            Amount = productEntity.Amount,
            Liked = liked,
            ImageData = Convert.ToBase64String(imageData)
        };
    }
}
