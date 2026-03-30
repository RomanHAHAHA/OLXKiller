using Common.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductsService.Domain.Dtos;

namespace ProductsService.Application.Features.ProductCharacteristics.Set;

[ApiController]
[Route("api/products")]
public class SetProductCharacteristicsController(IMediator mediator) : ControllerBase
{
    [HttpPost("{productId:guid}/characteristics")]
    [Authorize]
    public async Task<IActionResult> AddCharacteristicsAsync(
        Guid productId,
        [FromBody] List<ProductCharacteristicViewDto> characteristics,
        CancellationToken cancellationToken)
    {
        var command = new SetProductCharacteristicsCommand(productId, characteristics);
        var response = await mediator.Send(command, cancellationToken);
        return this.HandleResponse(response);
    }
}