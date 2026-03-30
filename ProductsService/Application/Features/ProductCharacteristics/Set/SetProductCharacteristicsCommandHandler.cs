using Common.Domain.Models.Results;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProductsService.Domain.Dtos;
using ProductsService.Domain.Entities;
using ProductsService.Infrastructure.Persistence;

namespace ProductsService.Application.Features.ProductCharacteristics.Set;

public class SetProductCharacteristicsCommandHandler(
    ProductsDbContext dbContext) : IRequestHandler<SetProductCharacteristicsCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(SetProductCharacteristicsCommand request, CancellationToken cancellationToken)
    {
        if (ListHasDuplicates(request.Characteristics))
        {
            return ApiResponse.BadRequest("Collection has duplicate names.");
        }
        
        var product = await dbContext.Products
            .Include(p => p.Characteristics)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product is null)
        {
            return ApiResponse.NotFound(nameof(Product));
        }

        if (product.Characteristics.Any())
        {
            dbContext.ProductCharacteristics.RemoveRange(product.Characteristics);
        }
        
        var newCharacteristics = request.Characteristics.Select(pc => new ProductCharacteristic
        {
            Name = pc.Name,
            ProductId = product.Id,
            Value = pc.Value,
        });

        product.Characteristics.AddRange(newCharacteristics);
        
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException e) when (e.InnerException is SqlException { Number: 2601 })
        {
            return ApiResponse.Conflict("Duplication of feature names is not possible");
        }
        catch (Exception)
        {
            return ApiResponse.InternalServerError();
        }

        return ApiResponse.Ok();
    }
    
    private static bool ListHasDuplicates(List<ProductCharacteristicViewDto> characteristics)
    {
        return characteristics.GroupBy(c => c.Name).Any(g => g.Count() > 1);
    }
}