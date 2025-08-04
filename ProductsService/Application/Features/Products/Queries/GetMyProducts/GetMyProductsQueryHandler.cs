using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductsService.Infrastructure.Persistence;

namespace ProductsService.Application.Features.Products.Queries.GetMyProducts;

public class GetMyProductsQueryHandler(
    ProductsDbContext dbContext) : IRequestHandler<GetMyProductsQuery, List<MyProductDto>>
{
    public async Task<List<MyProductDto>> Handle(
        GetMyProductsQuery request, 
        CancellationToken cancellationToken)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Include(p => p.Images)
            .Where(p => p.UserId == request.UserId)
            .Select(p => new MyProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                MainImageName = p.Images
                    .Where(i => i.IsMain)
                    .Select(i => i.ImagePath)
                    .FirstOrDefault() ?? string.Empty,
                Rating = p.AverageRating,
            })
            .ToListAsync(cancellationToken);
    }
}