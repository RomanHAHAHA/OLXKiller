using Common.Domain.Interfaces;
using ProductsService.Domain.Entities;

namespace ProductsService.Domain.Interfaces;

public interface ICategoriesRepository : IRepository<Category, Guid>
{
    
}