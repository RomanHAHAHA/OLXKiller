using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OLXKiller.API.Extensions;
using OLXKiller.Application.Abstractions;
using OLXKiller.Application.Dtos.Product;
using OLXKiller.Domain.Extensions;
using OLXKiller.Domain.Models;

namespace OLXKiller.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController(
    IProductsService _productsService) : ControllerBase
{
    [HttpPost("create")]
    [Authorize]
    public async Task<IActionResult> CreateProduct(
        [FromForm] CreateProductDto productDto)
    {
        var response = await _productsService
            .CreateProductAsync(productDto, User.GetId());

        if (response.IsSuccess)
        {
            return Ok(response);
        }

        return NotFound(new { description = response.Description });
    }

    [HttpPost("add-images-to-product/{productId:guid}")]
    public async Task<IActionResult> AddImagesToProduct(
        ICollection<IFormFile> images, 
        Guid productId)
    {
        if (images is null || images.Count == 0)
        {
            return BadRequest(new { description = "No images provided." });
        }

        var imagesBytes = await Task.WhenAll(
            images.Select(i => i.OpenReadStream().ConvertToBytesAsync())
        );

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

        foreach (var item in result.Collection)
        {
            Console.WriteLine(item.Liked);
        }

        if (result.Collection.Count() == 0)
        {
            return NotFound();
        }

        return Ok(new { result });
    }

    [HttpPost("like/{productId:guid}")]
    [Authorize]
    public async Task<IActionResult> LikeProduct(Guid productId)
    {
        var response = await _productsService.LikeProduct(productId, User.GetId());

        if (response.IsSuccess)
        {
            return Ok();
        }

        return this.CreateResponse(response.Status, response.Description);
    }

    [HttpPost("un-like/{productId:guid}")]
    [Authorize]
    public async Task<IActionResult> UnLikeProduct(Guid productId)
    {
        var response = await _productsService.UnLikeProduct(productId, User.GetId());

        if (response.IsSuccess)
        {
            return Ok();
        }

        return NotFound(new { description = response.Description });
    }
}
