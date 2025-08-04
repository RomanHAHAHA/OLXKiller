using Common.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProductsService.Application.Features.ProductImages.Create;

[ApiController]
[Route("/api/product-images")]
public class AddImagesController(IMediator mediator) : ControllerBase
{
    [HttpPost("{productId:guid}")]
    [Authorize]
    public async Task<IActionResult> AddImagesAsync(
        [FromForm] List<IFormFile> images,
        Guid productId,
        CancellationToken cancellationToken)
    {
        var command = new AddImagesCommand(images, productId);
        var response = await mediator.Send(command, cancellationToken);
        return this.HandleResponse(response);
    }
}