using OLXKiller.Application.Dtos.Product;
using OLXKiller.Domain.Entities;

namespace OLXKiller.Application.Abstractions;

public interface IProductDtoFactory
{
    Task<CollectionProductDto> CreateCollectionDtoAsync(
        ProductEntity productEntity,
        Guid currentUserId);

    Task<SingleProductDto> CreateSingleDtoAsync(
        ProductEntity productEntity,
        Guid currentUserId);
}