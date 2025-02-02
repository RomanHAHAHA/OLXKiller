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
    IProductImagesRepository _productImagesRepository,
    IProductDtoFactory _productDtoFactory) : IProductsService
{
    public async Task<IBaseResponse<Guid>> CreateProductAsync(
        CreateProductDto productDto,
        Guid sellerId)
    {
        var seller = await _usersRepository
            .GetByIdAsync(sellerId, TrackingEnable.False);

        if (seller is null)
            return new BaseResponse<Guid>(
                HttpStatusCode.NotFound, "Seller was not found");

        var product = productDto.AsEntity();
        
        await _productsRepository.CreateAsync(product);

        return new BaseResponse<Guid>(
            HttpStatusCode.OK, data: product.Id);
    }

    public async Task<IBaseResponse> RemoveProductAsync(Guid productId)
    {
        var product = await _productsRepository.GetByIdAsync(productId);

        if (product is null)
            return new BaseResponse(
                HttpStatusCode.NotFound, "Product was not found");

        await _productsRepository.RemoveAsync(product);

        return new BaseResponse(HttpStatusCode.OK);
    }

    public async Task<IBaseResponse> AddImagesToProductAsync(
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

    public async Task<IBaseResponse> RemoveImageFromProductAsync(Guid imageId)
    {
        var productImage = await _productImagesRepository.GetByIdAsync(imageId);

        if (productImage is null)
            return new BaseResponse(
                HttpStatusCode.OK, "Image was not found");

        await _productImagesRepository.RemoveAsync(productImage);

        return new BaseResponse(HttpStatusCode.OK);
    }

    public async Task<PagedResult<CollectionProductDto>> GetProductCollectionAsync(
        ProductFilter productFilter,
        ProductSortParams sortParams,
        PageParams pageParams, 
        Guid currentUserId)
    {
        var pagedResult = await _productsRepository
            .GetPaigedCollectionAsync(productFilter, sortParams, pageParams);

        var collection = await Task.WhenAll(pagedResult.Collection
            .Select(p => _productDtoFactory.CreateCollectionDtoAsync(p, currentUserId)));

        return new PagedResult<CollectionProductDto>(
            collection,
            pagedResult.TotalCount);
    }

    public async Task<IBaseResponse<SingleProductDto>> GetProductInfoAsync(
        Guid productId, Guid currentUserId)
    {
        var prodcut = await _productsRepository.GetByIdForSingleDto(productId);

        if (prodcut is null)
            return new BaseResponse<SingleProductDto>(
                HttpStatusCode.OK, "Product was not found");

        var dto = await _productDtoFactory.CreateSingleDtoAsync(prodcut, currentUserId);

        return new BaseResponse<SingleProductDto>(
            HttpStatusCode.OK, data: dto);
    }

    public async Task<IBaseResponse> LikeProduct(Guid productId, Guid userId)
    {
        var user = await _usersRepository.GetByIdAsync(userId);

        if (user is null)
            return new BaseResponse(
                HttpStatusCode.NotFound, "User was not found");

        var product = await _productsRepository.GetByIdAsync(userId);

        if (product is null)
            return new BaseResponse(
                HttpStatusCode.NotFound, "Product was not found");

        product.UsersWhoLiked.Add(user);
        await _productsRepository.UpdateAsync(product);

        return new BaseResponse(HttpStatusCode.OK);
    }

    public async Task<IBaseResponse> UnLikeProduct(Guid productId, Guid userId)
    {
        var user = await _usersRepository.GetByIdAsync(userId);

        if (user is null)
            return new BaseResponse(
                HttpStatusCode.NotFound, "User was not found");

        var product = await _productsRepository.GetByIdAsync(userId);

        if (product is null)
            return new BaseResponse(
                HttpStatusCode.NotFound, "Product was not found");

        product.UsersWhoLiked.Remove(user);
        await _productsRepository.UpdateAsync(product);

        return new BaseResponse(HttpStatusCode.OK);
    }
}
