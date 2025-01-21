using OLXKiller.Application.Abstractions;
using OLXKiller.Application.Dtos.Product;
using OLXKiller.Domain.Abstractions.Models;
using OLXKiller.Domain.Abstractions.Repositories;
using OLXKiller.Domain.Entities;
using OLXKiller.Domain.Enums;
using OLXKiller.Domain.Models;
using System.Net;

namespace OLXKiller.Application.Services;

public class ProductsService(
    IProductsRepository _productsRepository,
    IUsersRepository _usersRepository,
    IProductImagesRepository _productImagesRepository) : IProductsService
{
    public async Task<IBaseResponse> CreateProduct(
        CreateProductDto productDto,
        Guid sellerId)
    {
        var seller = await _usersRepository
            .GetByIdAsync(sellerId, TrackingEnable.False);

        if (seller is null)
        {
            return new BaseResponse(
                HttpStatusCode.NotFound,
                "Seller was not found");
        }

        var product = productDto.AsEntity(sellerId);
        
        await _productsRepository.CreateAsync(product);

        return new BaseResponse(HttpStatusCode.OK);
    }

    public async Task<IBaseResponse> AddImagesToProduct(
        IEnumerable<byte[]> imagesBytes, 
        Guid productId)
    {
        var product = await _productsRepository.GetByIdAsync(productId);

        if (product is null)
        {
            return new BaseResponse(
                HttpStatusCode.NotFound,
                "Product was not found");
        }

        var imagesToAdd = imagesBytes
            .Select(b => new ProductImageEntity(b, productId));

        await _productImagesRepository.CreateRange(imagesToAdd);

        return new BaseResponse(HttpStatusCode.OK);
    }
}
