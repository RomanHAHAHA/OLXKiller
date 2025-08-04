using Common.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProductsService.Application.Features.ProductImages.Delete;

[ApiController]
[Route("/api/product-images")]
public class DeleteImageController(IMediator mediator) : ControllerBase
{
    [HttpDelete("{productImageId:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteImageAsync(
        Guid productImageId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteProductImage(productImageId);
        var response = await mediator.Send(command, cancellationToken);
        return this.HandleResponse(response);
    }
}