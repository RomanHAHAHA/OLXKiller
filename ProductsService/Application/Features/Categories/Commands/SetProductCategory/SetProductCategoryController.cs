using Common.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProductsService.Application.Features.Categories.Commands.SetProductCategory;

[ApiController]
[Route("api/products")]
public class SetProductCategoryController(IMediator mediator) : ControllerBase
{
    [HttpPatch("{productId:guid}/category")]
    [Authorize]
    public async Task<IActionResult> AddCategoryAsync(
        Guid productId,
        [FromQuery] Guid? categoryId,
        CancellationToken cancellationToken)
    {
        var command = new SetProductCategoryCommand(productId, categoryId);
        var response = await mediator.Send(command, cancellationToken);
        return this.HandleResponse(response);
    }
}