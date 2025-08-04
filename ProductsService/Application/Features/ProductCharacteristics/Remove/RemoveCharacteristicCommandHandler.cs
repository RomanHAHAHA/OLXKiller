using Common.Domain.Models.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductsService.Domain.Entities;
using ProductsService.Infrastructure.Persistence;

namespace ProductsService.Application.Features.ProductCharacteristics.Remove;

public class RemoveCharacteristicCommandHandler(
    ProductsDbContext dbContext) : IRequestHandler<RemoveCharacteristicCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(RemoveCharacteristicCommand request, CancellationToken cancellationToken)
    {
        var characteristic = await dbContext.ProductCharacteristics
            .FirstOrDefaultAsync(c => 
                    c.ProductId == request.ProductId && 
                    c.Name == request.Name, 
                cancellationToken);

        if (characteristic is null)
        {
            return ApiResponse.NotFound(nameof(ProductCharacteristic));
        }
        
        dbContext.ProductCharacteristics.Remove(characteristic);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse.Ok();
    }
}