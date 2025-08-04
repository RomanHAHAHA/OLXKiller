using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductsService.Infrastructure.Persistence;

namespace ProductsService.Application.Features.Products.Queries.GetProductImages;

public class GetProductImagesQueryHandler(
    ProductsDbContext dbContext) : IRequestHandler<GetProductImagesQuery, List<ProductImageDto>>
{
    public async Task<List<ProductImageDto>> Handle(
        GetProductImagesQuery request, 
        CancellationToken cancellationToken)
    {
        return await dbContext.ProductImages
            .Where(p => p.ProductId == request.ProductId)
            .Select(i => new ProductImageDto
            {
                Id = i.Id,
                ImageName = i.ImagePath,
                IsMain = i.IsMain
            })
            .ToListAsync(cancellationToken);
    }
}