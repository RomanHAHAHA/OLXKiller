using Common.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductsService.Application.Common.Dtos;

namespace ProductsService.Application.Features.Products.Commands.Create;

[ApiController]
[Route("api/products")]
public class CreateProductController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateProductAsync(
        [FromBody] ProductCreateDto productCreateDto,
        CancellationToken cancellationToken)
    {
        var command = new CreateProductCommand(User.GetId(), productCreateDto);
        var response = await mediator.Send(command, cancellationToken);
        return this.HandleResponse(response);
    }
}