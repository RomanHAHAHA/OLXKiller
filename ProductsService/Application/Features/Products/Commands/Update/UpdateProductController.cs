using Common.API.Extensions;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using ProductsService.Application.Common.Dtos;

namespace ProductsService.Application.Features.Products.Commands.Update;

[ApiController]
[Route("api/products")]
public class UpdateProductController(IMediator mediator) : ControllerBase
{
    [HttpPatch("{productId:guid}")]
    [Authorize]
    public async Task<IActionResult> UpdateProductAsync(
        Guid productId,
        [FromBody] ProductCreateDto productCreateDto,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProductCommand(User.GetId(), productId, productCreateDto);
        var response = await mediator.Send(command, cancellationToken);
        return this.HandleResponse(response);
    }
}