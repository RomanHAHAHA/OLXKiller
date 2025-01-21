using OLXKiller.Domain.Entities;

namespace OLXKiller.Domain.Abstractions.Repositories;

public interface IProductImagesRepository : IRepository<ProductImageEntity>
{
    Task CreateRange(IEnumerable<ProductImageEntity> images);
}
