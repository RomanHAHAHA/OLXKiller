using Common.API.Extensions;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace ProductsService.Application.Features.ProductCharacteristics.Update;

[ApiController]
[Route("api/products")]
public class UpdateProductCharacteristicController(IMediator mediator) : ControllerBase
{
    [HttpPatch("{productId:guid}/characteristics")]
    [Authorize]
    public async Task<IActionResult> UpdateProductCharacteristicAsync(
        Guid productId,
        ProductCharacteristicUpdateDto updateDto,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCharacteristicCommand(productId, updateDto);
        var response = await mediator.Send(command, cancellationToken);
        return this.HandleResponse(response);
    }
}