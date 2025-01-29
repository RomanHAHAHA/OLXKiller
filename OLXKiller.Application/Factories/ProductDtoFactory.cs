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

    public async Task<SingleProductDto> CreateSingleDtoAsync(
        ProductEntity productEntity,
        Guid currentUserId)
    {
        List<byte[]> bytes = productEntity.Images.Count() == 0 ?
            [await _imageManager.GetDefaultBytesAsync(_imageOptions.Value.DefaultProductImageName)] :
            productEntity.Images.Select(i => i.Data).ToList();

        var liked = false;

        if (!currentUserId.Equals(Guid.Empty))
        {
            liked = productEntity.UsersWhoLiked
                .FirstOrDefault(like => like.UserId == currentUserId) is not null;
        }

        var sellerAvatar = Convert.ToBase64String(
            productEntity?.Seller?.Avatar?.Data ??
            await _imageManager.GetDefaultBytesAsync(_imageOptions.Value.DefaultProductImageName));

        return new SingleProductDto
        {
            Id = productEntity.Id,
            Name = productEntity.Name,
            Description = productEntity.Description,
            Price = productEntity.Price,
            Amount = productEntity.Amount,
            SellerId = productEntity.SellerId,
            SellerNickName = productEntity?.Seller?.NickName ?? string.Empty,
            Liked = liked,
            ImageStrings = bytes.Select(Convert.ToBase64String),
            SellerAvatar = sellerAvatar
        };
    }
}
