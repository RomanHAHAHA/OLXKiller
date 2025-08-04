using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductsService.Infrastructure.Persistence;

namespace ProductsService.Application.Features.Products.Queries.GetProductCharacteristics;

public class GetProductCharacteristicsQueryHandler(
    ProductsDbContext dbContext) : 
    IRequestHandler<GetProductCharacteristicsQuery, List<ProductCharacteristicDto>>
{
    public async Task<List<ProductCharacteristicDto>> Handle(
        GetProductCharacteristicsQuery request, 
        CancellationToken cancellationToken)
    {
        return await dbContext.ProductCharacteristics
            .AsNoTracking()
            .Where(pc => pc.ProductId == request.ProductId)
            .Select(pc => new ProductCharacteristicDto
            {
                Id = pc.Id,
                Name = pc.Name,
                Value = pc.Value
            })
            .ToListAsync(cancellationToken);
    }
}