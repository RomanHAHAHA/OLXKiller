using OLXKiller.Domain.Abstractions.Repositories;
using OLXKiller.Domain.Entities;
using OLXKiller.Persistence.Contexts;

namespace OLXKiller.Persistence.Repositories;

public class ProductsRepository(AppDbContext appDbContext) :
    Repository<ProductEntity>(appDbContext),
    IProductsRepository
{
}
