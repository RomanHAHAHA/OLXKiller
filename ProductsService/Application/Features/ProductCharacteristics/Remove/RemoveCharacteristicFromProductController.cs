using Common.API.Extensions;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace ProductsService.Application.Features.ProductCharacteristics.Remove;

[ApiController]
[Route("api/products")]
public class RemoveCharacteristicFromProductController(IMediator mediator) : ControllerBase
{
    [HttpDelete("{productId:guid}/characteristics/{name}")]
    [Authorize]
    public async Task<IActionResult> DeleteCharacteristicAsync(
        Guid productId,
        string name,
        CancellationToken cancellationToken)
    {
        var command = new RemoveCharacteristicCommand(productId, name);
        var response = await mediator.Send(command, cancellationToken);
        return this.HandleResponse(response);
    }
}