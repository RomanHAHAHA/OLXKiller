using Common.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProductsService.Application.Features.Categories.Commands.RemoveCategoryFromProduct;

[ApiController]
[Route("api/products")]
public class RemoveCategoryFromProductController(IMediator mediator) : ControllerBase
{
    [HttpDelete("{productId:guid}/categories/{categoryId:guid}")]
    [Authorize]
    public async Task<IActionResult> RemoveCategoryAsync(
        Guid productId,
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveCategoryCommand(productId, categoryId);
        var response = await mediator.Send(command, cancellationToken);
        return this.HandleResponse(response);
    }
}