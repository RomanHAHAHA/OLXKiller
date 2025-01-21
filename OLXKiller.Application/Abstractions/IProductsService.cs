using OLXKiller.Application.Dtos.Product;
using OLXKiller.Domain.Abstractions.Models;

namespace OLXKiller.Application.Abstractions;

public interface IProductsService
{
    Task<IBaseResponse> CreateProduct(
        CreateProductDto productDto,
        Guid sellerId);

    Task<IBaseResponse> AddImagesToProduct(
        IEnumerable<byte[]> imagesBytes,
        Guid productId);
}
