using Common.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProductsService.Application.Features.ProductImages.SetMain;

[ApiController]
[Route("/api/product-images")]
public class SetMainImageController(IMediator mediator) : ControllerBase
{
    [HttpPatch("{imageId:guid}")]
    [Authorize]
    public async Task<IActionResult> SetMainImageAsync(
        Guid imageId,
        CancellationToken cancellationToken)
    {
        var command = new SetMainImageCommand(imageId);
        var response = await mediator.Send(command, cancellationToken);
        return this.HandleResponse(response);
    }
}