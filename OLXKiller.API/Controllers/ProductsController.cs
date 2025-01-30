using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Writers;
using OLXKiller.API.Authentication;
using OLXKiller.API.Extensions;
using OLXKiller.Application.Abstractions;
using OLXKiller.Application.Dtos.Product;
using OLXKiller.Domain.Enums;
using OLXKiller.Domain.Extensions;
using OLXKiller.Domain.Models;
using System.Linq.Expressions;

namespace OLXKiller.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController(
    IProductsService _productsService) : ControllerBase
{
    [HttpPost("create")]
    [Authorize]
    public async Task<IActionResult> CreateProduct(
        [FromBody] CreateProductDto productDto)
    {
        var response = await _productsService
            .CreateProductAsync(productDto, User.GetId());

        if (response.IsSuccess)
        {
            return Ok(new { productId = response.Data });
        }

        return NotFound(new { description = response.Description });
    }

    [HttpDelete("{productId:guid}")]
    [HasPermission(Permission.DeleteAllProducts)]
    public async Task<IActionResult> DeleteProduct(Guid productId)
    {
        throw new NotImplementedException();
    }

    [HttpPost("add-images-to-product/{productId:guid}")]
    [Authorize]
    public async Task<IActionResult> AddImagesToProduct(
        ICollection<IFormFile> images,
        Guid productId)
    {
        if (images is null || images.Count == 0)
        {
            return BadRequest(new { description = "No images provided." });
        }

        var imagesBytes = images
            .Select(image => image.OpenReadStream().ConvertToBytes());

        var response = await _productsService
            .AddImagesToProductAsync(imagesBytes, productId);

        if (response.IsSuccess)
        {
            return Ok();
        }

        return NotFound(new { description = response.Description });
    }

    [HttpGet()]
    public async Task<IActionResult> GetAllProducts(
        [FromQuery] ProductFilter filter,
        [FromQuery] ProductSortParams sortParams,
        [FromQuery] PageParams pageParams)
    {
        var result = await _productsService
            .GetProductCollectionAsync(filter, sortParams, pageParams, User.GetId());

        if (result.Collection.Count() == 0)
        {
            return NotFound();
        }

        return Ok(new { result });
    }

    [HttpGet("userId={userId:guid}")]
    public async Task<IActionResult> GetUserProducts(Guid userId)
    {
        var products = await _productsService.GetUserProductsAsync(userId);

        if (products.Count() == 0)
        {
            return NotFound();
        }

        return Ok(new { products });
    }
    
    [HttpGet("productId={productId:guid}")]
    public async Task<IActionResult> GetProductInfo(Guid productId)
    {
        var response = await _productsService.GetProductInfoAsync(productId, User.GetId());

        if (response.IsSuccess)
        {
            return Ok(new { data = response.Data });
        }

        return NotFound(new { description = response.Description });
    }

    [HttpPost("like/{productId:guid}")]
    [Authorize]
    public async Task<IActionResult> LikeProduct(Guid productId)
    {
        var response = await _productsService.LikeProduct(productId, User.GetId());

        if (response.IsFailure)
        {
            return this.HandleResponse(response);
        }

        return Ok();
    }

    [HttpPost("un-like/{productId:guid}")]
    [Authorize]
    public async Task<IActionResult> UnLikeProduct(Guid productId)
    {
        var response = await _productsService.UnLikeProduct(productId, User.GetId());

        if (response.IsFailure)
        {
            return this.HandleResponse(response);
        }

        return Ok();
    }
}
