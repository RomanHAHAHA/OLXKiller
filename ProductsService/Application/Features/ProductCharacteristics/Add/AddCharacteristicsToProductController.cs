using Common.API.Extensions;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using ProductsService.Domain.Dtos;

namespace ProductsService.Application.Features.ProductCharacteristics.Add;

[ApiController]
[Route("api/products")]
public class AddCharacteristicsToProductController(IMediator mediator) : ControllerBase
{
    [HttpPost("{productId:guid}/characteristics")]
    [Authorize]
    public async Task<IActionResult> AddCharacteristicsAsync(
        Guid productId,
        [FromBody] List<ProductCharacteristicViewDto> characteristics,
        CancellationToken cancellationToken)
    {
        var command = new AddCharacteristicsCommand(productId, characteristics);
        var response = await mediator.Send(command, cancellationToken);
        return this.HandleResponse(response);
    }
}