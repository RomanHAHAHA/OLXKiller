using OLXKiller.Application.Dtos.Product;
using OLXKiller.Domain.Abstractions.Models;
using OLXKiller.Domain.Models;

namespace OLXKiller.Application.Abstractions;

public interface IProductsService
{
    Task<IBaseResponse> CreateProductAsync(
        CreateProductDto productDto,
        Guid sellerId);

    Task<IBaseResponse> AddImagesToProductAsync(
        IEnumerable<byte[]> imagesBytes,
        Guid productId);

    Task<PagedResult<CollectionProductDto>> GetProductCollectionAsync(
        ProductFilter productFilter,
        ProductSortParams sortParams,
        PageParams pageParams);
}
