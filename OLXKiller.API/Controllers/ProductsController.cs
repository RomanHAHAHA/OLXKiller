using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OLXKiller.API.Extensions;
using OLXKiller.Application.Abstractions;
using OLXKiller.Application.Dtos.Product;
using OLXKiller.Domain.Extensions;

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
        var response = await _productsService.CreateProduct(productDto, User.GetId());

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
            .AddImagesToProduct(imagesBytes, productId);

        if (response.IsSuccess)
        {
            return Ok();
        }

        return NotFound(new { description = response.Description });
    }

    /*[HttpGet("all")]
    public async Task<IEnumerable<object>> GetAllProducts()
    {
        return [];
    }*/
}
