using Common.API.Extensions;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace ProductsService.Application.Features.Products.Commands.Delete;

[ApiController]
[Route("api/products")]
public class DeleteProductController(IMediator mediator) : ControllerBase
{
    [HttpDelete("{productId:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteProductAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteProductCommand(User.GetId(), productId);
        var response = await mediator.Send(command, cancellationToken);
        return this.HandleResponse(response);
    }
}