using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductsService.Domain.Entities;
using ProductsService.Infrastructure.Persistence;

namespace ProductsService.Application.Features.Categories.Queries.GetDbCategories;

public class GetAllDbCategoriesQueryHandler(
    ProductsDbContext dbContext) : IRequestHandler<GetDbCategoriesQuery, List<Category>>
{
    public async Task<List<Category>> Handle(GetDbCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Categories
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}