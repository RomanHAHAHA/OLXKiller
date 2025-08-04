using Common.Domain.Models.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductsService.Domain.Entities;
using ProductsService.Infrastructure.Persistence;

namespace ProductsService.Application.Features.Products.Queries.GetProductBase;

public class GetProductBaseQueryHandler(
    ProductsDbContext dbContext) : IRequestHandler<GetProductBaseQuery, ApiResponse<Product>>
{
    public async Task<ApiResponse<Product>> Handle(GetProductBaseQuery request, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);
        
        return product ?? ApiResponse<Product>.NotFound(nameof(Product));
    }
}