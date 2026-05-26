using ProductsService.Application.Common.Dtos;
using ProductsService.Application.Features.Products.Commands.Update;
using ProductsService.Domain.Entities;

namespace ProductsService.Domain.Extensions;

public static class ProductExtensions
{
    public static void UpdateFromCreateDto(this Product product, ProductCreateDto productCreateDto)
    {
        product.Name = productCreateDto.Name;
        product.Description = productCreateDto.Description;
        product.Price = productCreateDto.Price!.Value;
        product.StockQuantity = productCreateDto.StockQuantity!.Value;
    }
    
    public static void CopyProperties(this Product product, OldProductProperties targetProduct)
    {
        product.Name = targetProduct.Name;
        product.Description = targetProduct.Description;
        product.Price = targetProduct.Price;
        product.StockQuantity = targetProduct.StockQuantity;
    }
}