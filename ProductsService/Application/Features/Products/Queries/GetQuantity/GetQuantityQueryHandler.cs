using Common.Domain.Models.Results;
using MediatR;
using ProductsService.Domain.Entities;
using ProductsService.Domain.Interfaces;

namespace ProductsService.Application.Features.Products.Queries.GetQuantity;

public class GetQuantityQueryHandler(
    IProductsRepository productsRepository) : IRequestHandler<GetQuantityQuery, ApiResponse<int>>
{
    public async Task<ApiResponse<int>> Handle(GetQuantityQuery request, CancellationToken cancellationToken)
    {
        var productQuantity = await productsRepository
            .GetQuantityById(request.ProductId, cancellationToken);

        return productQuantity is null ? 
            ApiResponse<int>.NotFound(nameof(Product)) : 
            ApiResponse<int>.Ok(productQuantity.Value);
    }
}