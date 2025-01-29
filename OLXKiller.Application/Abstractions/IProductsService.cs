using OLXKiller.Application.Dtos.Product;
using OLXKiller.Domain.Abstractions.Models;
using OLXKiller.Domain.Models;

namespace OLXKiller.Application.Abstractions;

public interface IProductsService
{
    Task<IBaseResponse<Guid>> CreateProductAsync(
        CreateProductDto productDto,
        Guid sellerId);

    Task<IBaseResponse> RemoveProductAsync(Guid productId);

    Task<IBaseResponse> AddImagesToProductAsync(
        IEnumerable<byte[]> imagesBytes,
        Guid productId);

    Task<IBaseResponse> RemoveImageFromProductAsync(Guid imageId);

    Task<PagedResult<CollectionProductDto>> GetProductCollectionAsync(
        ProductFilter productFilter,
        ProductSortParams sortParams,
        PageParams pageParams, 
        Guid currentUserId);

    Task<IEnumerable<CollectionProductDto>> GetUserProductsAsync(Guid userId);

    Task<IBaseResponse<SingleProductDto>> GetProductInfoAsync(
        Guid productId, Guid currentUserId);

    Task<IBaseResponse> LikeProduct(Guid productId, Guid userId);

    Task<IBaseResponse> UnLikeProduct(Guid productId, Guid userId);
}
