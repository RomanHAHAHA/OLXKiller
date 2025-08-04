using Common.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProductsService.Application.Features.Categories.Commands.AddCategoryToProduct;

[ApiController]
[Route("api/products")]
public class AddCategoryToProductController(IMediator mediator) : ControllerBase
{
    [HttpPost("{productId:guid}/categories/{categoryId:guid}")]
    [Authorize]
    public async Task<IActionResult> AddCategoryAsync(
        Guid productId,
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        var command = new AddCategoryCommand(productId, categoryId);
        var response = await mediator.Send(command, cancellationToken);
        return this.HandleResponse(response);
    }
}