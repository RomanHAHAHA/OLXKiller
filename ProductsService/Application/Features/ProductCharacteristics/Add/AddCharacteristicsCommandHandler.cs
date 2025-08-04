using Common.Domain.Models.Results;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProductsService.Domain.Dtos;
using ProductsService.Domain.Entities;
using ProductsService.Infrastructure.Persistence;

namespace ProductsService.Application.Features.ProductCharacteristics.Add;

public class AddCharacteristicsCommandHandler(
    ProductsDbContext dbContext) : IRequestHandler<AddCharacteristicsCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(AddCharacteristicsCommand request, CancellationToken cancellationToken)
    {
        if (ListHasDuplicates(request.Characteristics))
        {
            return ApiResponse.BadRequest("Collection has duplicate names.");
        }
        
        if (!await ProductExistsAsync(request.ProductId, cancellationToken))
        {
            return ApiResponse.NotFound(nameof(Product));
        }
        
        var characteristics = request.Characteristics
            .Select(c => new ProductCharacteristic
            {
                ProductId = request.ProductId,
                Name = c.Name,
                Value = c.Value
            }).ToList();
        
        await dbContext.ProductCharacteristics.AddRangeAsync(characteristics, cancellationToken);

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

    private async Task<bool> ProductExistsAsync(Guid productId, CancellationToken cancellationToken)
    {
        return await dbContext.Products.AnyAsync(p => p.Id == productId, cancellationToken);
    }
}