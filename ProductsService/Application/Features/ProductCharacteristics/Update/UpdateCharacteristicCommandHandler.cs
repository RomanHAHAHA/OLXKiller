using Common.Domain.Models.Results;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProductsService.Domain.Entities;
using ProductsService.Infrastructure.Persistence;

namespace ProductsService.Application.Features.ProductCharacteristics.Update;

public class UpdateCharacteristicCommandHandler(
    ProductsDbContext dbContext) : IRequestHandler<UpdateCharacteristicCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(UpdateCharacteristicCommand request, CancellationToken cancellationToken)
    {
        var characteristic = await GetCharacteristicAsync(request, cancellationToken);

        if (characteristic is null)
        {
            return ApiResponse.NotFound(nameof(ProductCharacteristic));
        }
        
        characteristic.Name = request.CharacteristicDto.NewName;
        characteristic.Value = request.CharacteristicDto.Value;
        
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException e) when (e.InnerException is SqlException { Number: 2601 })
        {
            return ApiResponse.Conflict($"Feature \"{request.CharacteristicDto.NewName}\" is already exists.");
        }
        catch (Exception)
        {
            return ApiResponse.InternalServerError();
        }

        return ApiResponse.Ok();
    }
    
    private async Task<ProductCharacteristic?> GetCharacteristicAsync(
        UpdateCharacteristicCommand request,
        CancellationToken cancellationToken)
    {
        return await dbContext.ProductCharacteristics
            .FirstOrDefaultAsync(c => 
                    c.ProductId == request.ProductId && 
                    c.Name == request.CharacteristicDto.OldName,
                cancellationToken);
    }
}